using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;
using ServiceStack;
using ServiceStack.Configuration;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using static System.String;

namespace YCIOT.ModbusPoll.RtuOverTcp.Bridge
{

    //ModbusTCP转ModbusRTU实现思路
    //https://www.pianshen.com/article/1535323386/

    //MODBUS TCP和MODBUS RTU的差别
    //https://www.bbsmax.com/A/QW5YeGVO5m/

    //Modbus tcp 格式说明 通讯机制 附C#测试工具用于学习，测试
    //https://www.cnblogs.com/dathlin/p/8007297.html

    //C# 开发Modbus Rtu客户端 modbus测试Demo，Modbus 串口通信 , 虚拟MODBUS-RTU测试
    //https://www.cnblogs.com/dathlin/p/8974215.html

    //工业通信的开源项目 HslCommunication 介绍
    //https://www.cnblogs.com/dathlin/p/10390311.html

    //https://github.com/jchristn/TcpTest/blob/master/Server/ManyMetadata.cs

    //Rapid SCADA Modbus Parser
    //http://rapidscada.net/modbus/ModbusParser.aspx
    public class TcpBridge
    {
        public static bool RunForever = true;
        public static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static readonly CancellationToken Token = TokenSource.Token;
        public static IPAddress ManyIpAddress;
        public static TcpListener ManyListener;
        public static readonly ConcurrentDictionary<string, ManyMetadata> ManyClients = new ConcurrentDictionary<string, ManyMetadata>();
        public static readonly ConcurrentQueue<byte[]> ManyQueue = new ConcurrentQueue<byte[]>();

        public static IPAddress OneIpAddress;
        public static TcpListener OneListener;

        public static readonly ConcurrentDictionary<string, OneMetadata> OneClients = new ConcurrentDictionary<string, OneMetadata>();
        public static readonly ConcurrentQueue<byte[]> OneQueue = new ConcurrentQueue<byte[]>();
        public static readonly bool useLinkId = new AppSettings().Get<bool>("Modbus.UserLinkId");

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");
        private static readonly bool isUseStrictCheckMode = new AppSettings().Get<bool>("Modbus.IsUseStrictCheckMode");

        private static readonly int modbusTimeout = new AppSettings().Get<int>("Modbus.Timeout", 3000);


        public TcpBridge(string localIp, int tcpPortOne, int tcpPortMany)
        {
            OneIpAddress = IPAddress.Parse(localIp);
            OneListener = new TcpListener(OneIpAddress, tcpPortOne);

            ManyIpAddress = IPAddress.Parse(localIp);
            ManyListener = new TcpListener(ManyIpAddress, tcpPortMany);

            OneListener.Start();
            ManyListener.Start();

            Task.Run(() => OneAcceptConnections(Token), Token);
            Task.Run(() => ManyAcceptConnections(Token), Token);

            #region 1-->m 转发
            Task.Run(() =>
            {
                while (RunForever)
                {
                    try
                    {
                        if (!ManyQueue.TryDequeue(out var data))
                        {
                            Thread.Sleep(20);
                            continue;
                        };

                        var policy = new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTime.Now.AddMinutes(10)
                        };
                        foreach (var cur in ManyClients)
                        {
                            ClientInfo.cache.Set("one", "", policy);
                            var key = cur.Value.TcpClient.Client.RemoteEndPoint.ToString();
                            if (!ManyClients.TryGetValue(key, out var value)) continue;

                            if (useLinkId)
                            {
                                if (ClientInfo.LinkId.HasValue && value.LinkId.HasValue)
                                {
                                    if (value.LinkId == ClientInfo.LinkId)
                                    {
                                        lock (value.SendLock)
                                        {
                                            value.NetworkStream.Write(data, 0, data.Length);
                                            value.NetworkStream.Flush();
                                        }
                                        $"[1->M]----[{ClientInfo.LinkId}-{key}]-{BytesToHexString(data)}----".Info();


                                        lock (ClientInfo.locker)
                                        {
                                            ClientInfo.IpAddress = key;
                                            ClientInfo.RequestTime = DateTime.Now;
                                        }

                                        break;

                                    }
                                }
                            }
                            else
                            {
                                lock (value.SendLock)
                                {
                                    value.NetworkStream.Write(data, 0, data.Length);
                                    value.NetworkStream.Flush();

                                    $"[1->M]----已转发到[{key}]-{BytesToHexString(data)}".Info();
                                    ClientInfo.IpAddress = key;
                                    ClientInfo.RequestTime = DateTime.Now;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                }
            });
            #endregion

            #region m-->1 转发
            Task.Run(() =>
                 {
                     while (RunForever)
                     {
                         try
                         {
                             if (!OneQueue.TryDequeue(out var data))
                             {
                                 Thread.Sleep(100);
                                 continue;
                             };

                             foreach (var cur in OneClients)
                             {
                                 var key = cur.Value.TcpClient.Client.RemoteEndPoint.ToString();

                                 if (!OneClients.TryGetValue(key, out var value))
                                 {
                                     "One Client has removed!".Warn();
                                     continue;
                                 }

                                 lock (value.SendLock)
                                 {
                                     value.NetworkStream.Write(data, 0, data.Length);
                                     value.NetworkStream.Flush();
                                 }
                             }
                         }
                         catch (Exception e)
                         {
                             Logger.Error(e);
                                //throw;
                            }
                     }
                 });
            #endregion
        }

        #region  One Receiver->ManyQueue,Many Receiver->OneQueue
        private static async Task OneDataReceiver(OneMetadata md)
        {
            var header = "[" + md.TcpClient.Client.RemoteEndPoint.ToString() + "]";
            try
            {
                while (true)
                {
                    if (!IsClientConnected(md.TcpClient))
                    {
                        if (isDebug)
                        {
                            Logger.Warn(header + " client no longer connected");
                        }
                        break;
                    }

                    if (Token.IsCancellationRequested)
                    {
                        if (isDebug)
                        {
                            Logger.Warn(header + " cancellation requested");
                        }

                        break;
                    }

                    var data = await DataReadAsync(md.TcpClient, Token);
                    if (data == null || data.Length < 1)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var length = data.Length;

                    var crc32 = data.Take(length - 2).ToArray().CRC16();
                    if (data[length - 2] == crc32[1] && data[length - 1] == crc32[0])
                    {
                        ManyQueue.Enqueue(data.Take(data.Length).ToArray());
                    }

                }
            }
            catch (Exception e)
            {
                $"one client excetion {e.Message}, {e.StackTrace}".Error();
            }
            OneClients.TryRemove(md.TcpClient.Client.RemoteEndPoint.ToString(), out _);
            md.Dispose();
        }

        private static async Task ManyDataReceiver(ManyMetadata md)
        {
            var header = "[" + md.TcpClient.Client.RemoteEndPoint.ToString() + "]";
            $"{header} many data receiver started".Info();

            try
            {
                while (true)
                {
                    if (!IsClientConnected(md.TcpClient))
                    {
                        Logger.Error(header + $" client no longer connected[{md.LinkId}]");
                        break;
                    }

                    if (Token.IsCancellationRequested)
                    {
                        //if (isDebug)
                        Logger.Info(header + " cancellation requested");
                        break;
                    }
                    var data = await DataReadAsync(md.TcpClient, Token);

                    if (data == null || data.Length < 1)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var length = data.Length;

                    var linkId = 0;

                    var policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(10)
                    };

                    switch (data.Length)
                    {
                        case 3:
                            linkId = data[2];

                            lock (ClientInfo.locker)
                            {
                                ClientInfo.cache.Set("many", "", policy);
                            }

                            switch (data[1])
                            {
                                case 0xC7:

                                    $"收到连接[3][{ManyClients.Count}][{linkId}]注册包".Info();
                                    md.LinkId = linkId;

                                    var policy1 = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddSeconds(30 * 3)
                                    };

                                    lock (ClientInfo.locker)
                                    {
                                        ClientInfo.cache.Set(linkId.ToString(), "", policy1);
                                    }
                                    break;

                                case 0xC8:
                                    //if (isDebug)
                                    {
                                        Logger.Info($"收到连接[3][{ManyClients.Count}][{linkId}]心跳包");
                                    }

                                    md.LinkId = linkId;

                                    var policy2 = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddSeconds(30 * 3)
                                    };
                                    lock (ClientInfo.locker)
                                    {
                                        ClientInfo.cache.Set(linkId.ToString(), "", policy2);
                                    }
                                    break;
                            }
                            break;

                        case 4:
                            var linkIdHigh = data[2];
                            var linkIdLow = data[3];

                            linkId = (linkIdHigh << 8) | (linkIdLow);
                            lock (ClientInfo.locker)
                            {
                                ClientInfo.cache.Set("many", "", policy);
                            }

                            switch (data[1])
                            {
                                case 0xC7:

                                    $"收到连接[4][{ManyClients.Count}][{linkId}]注册包".Info();
                                    md.LinkId = linkId;

                                    var policy1 = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddSeconds(30 * 3)
                                    };
                                    lock (ClientInfo.locker)
                                    {
                                        ClientInfo.cache.Set(linkId.ToString(), "", policy1);
                                    }
                                    break;

                                case 0xC8:
                                    //if (isDebug)
                                    Logger.Info($"收到连接[4][{ManyClients.Count}][{linkId}]心跳包");
                                    md.LinkId = linkId;

                                    var policy2 = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddSeconds(30 * 3)
                                    };
                                    lock (ClientInfo.locker)
                                    {
                                        ClientInfo.cache.Set(linkId.ToString(), "", policy2);
                                    }
                                    break;
                            }
                            break;

                        default:
                            if (length <= 5) continue;
                            Logger.Info($"[M][{md.TcpClient.Client.RemoteEndPoint}]:{BytesToHexString(data)}");
                            var crc32 = data.Take(length - 2).ToArray().CRC16();
                            if (data[length - 2] == crc32[1] && data[length - 1] == crc32[0])
                            {
                                if (useLinkId)
                                {
                                    if (md.TcpClient.Client.RemoteEndPoint.ToString() == ClientInfo.IpAddress)
                                    {
                                        var dt = new DateTime();

                                        lock (ClientInfo.locker)
                                        {
                                            dt = ClientInfo.RequestTime;
                                        }
                                        var ts = DateTime.Now - dt;

                                        if (ts.TotalMilliseconds < modbusTimeout + 500)
                                        {
                                            lock (ClientInfo.locker)
                                            {
                                                if (!isUseStrictCheckMode || (ClientInfo.ExpectedType == data[1] && ClientInfo.ExpectedDataLen == data[2]))
                                                {
                                                    OneQueue.Enqueue(data.Take(data.Length).ToArray());
                                                    $"[M->1]{header}:{ BytesToHexString(data)}".Info();
                                                }
                                                else
                                                {
                                                    Logger.Warn($"[M->1] [{ClientInfo.ExpectedType}][{ClientInfo.ExpectedDataLen}],{BytesToHexString(data)}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn($"TimeOut[{ts.TotalMilliseconds}][M->1] [{ClientInfo.ExpectedType}][{ClientInfo.ExpectedDataLen}],{BytesToHexString(data)}");
                                        }
                                    }
                                    else
                                    {

                                        var dt = new DateTime();

                                        lock (ClientInfo.locker)
                                        {
                                            dt = ClientInfo.RequestTime;
                                        }

                                        var ts = DateTime.Now - dt;

                                        if (ts.TotalMilliseconds < modbusTimeout)
                                        {
                                            //Logger.Error($"[M->?][{md.TcpClient.Client.RemoteEndPoint}] :[{ ClientInfo.ManyIpAddress}]");
                                            //$"[M->?]{header}:{ BytesToHexString(data)}".Info();
                                        }
                                        else
                                        {
                                            //Logger.Error("[M->?] timeout!");
                                        }
                                    }
                                }
                                else
                                {
                                    OneQueue.Enqueue(data.Take(data.Length).ToArray());
                                    $"[M->1]{header}: { BytesToHexString(data)}".Info();
                                }

                            }
                            else
                            {
                                Logger.Error($"CRC Error:{crc32[1]}:{crc32[0]}");
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(
                    Environment.NewLine +
                    header +
                    " ManyDataReceiver Exception: " +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine +
                    e.StackTrace
                    );
            }
            //if (isDebug)
            Logger.Info(header + " data receiver terminating");

            ManyClients.TryRemove(md.TcpClient.Client.RemoteEndPoint.ToString(), out _);
            md.Dispose();
        }

        #endregion

        #region 基础方法

        private static async Task OneAcceptConnections(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await OneListener.AcceptTcpClientAsync();
                    var md = new OneMetadata(client);

                    $"One客户端已连接,IP:{client.Client.RemoteEndPoint}".Info();

                    OneClients.TryAdd(client.Client.RemoteEndPoint.ToString(), md);
                    await Task.Run(() => OneDataReceiver(md), md.Token);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }

            }
        }

        private static async Task ManyAcceptConnections(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await ManyListener.AcceptTcpClientAsync();
                    Logger.Warn($"设备已连接,IP:{client.Client.RemoteEndPoint}");
                    var md = new ManyMetadata(client);
                    ManyClients.TryAdd(client.Client.RemoteEndPoint.ToString(), md);

#pragma warning disable 4014
                    Task.Run(() => ManyDataReceiver(md), md.Token);
#pragma warning restore 4014
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            }
        }


        private static void DisposeServer()
        {
            try
            {
                if (OneClients != null && OneClients.Count > 0)
                {
                    foreach (var (key, value) in OneClients)
                    {
                        if (isDebug)
                        {
                            Logger.Info("Disconnecting " + key);
                        }

                        value.Dispose();
                    }
                }

                TokenSource.Cancel();
                TokenSource.Dispose();

                if (OneListener?.Server != null)
                {
                    OneListener.Server.Close();
                    OneListener.Server.Dispose();
                }

                OneListener?.Stop();

                if (ManyClients != null && ManyClients.Count > 0)
                {
                    foreach (var (key, value) in ManyClients)
                    {
                        if (isDebug)
                            Logger.Info("Disconnecting " + key);
                        value.Dispose();
                    }
                }

                TokenSource.Cancel();
                TokenSource.Dispose();

                if (ManyListener?.Server != null)
                {
                    ManyListener.Server.Close();
                    ManyListener.Server.Dispose();
                }

                ManyListener?.Stop();
            }
            catch (Exception e)
            {
                Logger.Error(
                    Environment.NewLine +
                    "Dispose Exception:" +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine);
            }
        }

        public static string BytesToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2} ", b);
            }
            return sb.ToString();
        }


        private static async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        {
            if (token.IsCancellationRequested) throw new OperationCanceledException();

            var stream = client.GetStream();
            if (!stream.CanRead) return null;
            if (!stream.DataAvailable) return null;

            var buffer = new byte[1024];

            await using var ms = new MemoryStream();

            IAppSettings appSettings = new AppSettings();
            var timeout = appSettings.Get<int>("Modbus.Timeout", 3000);
            DateTime start = DateTime.Now;

            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                if (read > 0)
                {
                    DateTime end = DateTime.Now;
                    if (end.Subtract(start).TotalMilliseconds > timeout)
                    {
                        "获取数据超时".Info();
                        return null;
                    }

                    ms.Write(buffer, 0, read);
                    return ms.ToArray();
                }
                else
                {
                    throw new SocketException();
                }
            }
        }

        private static bool IsClientConnected(TcpClient client)
        {
            if (client.Connected)
            {
                if ((client.Client.Poll(0, SelectMode.SelectWrite)) && (!client.Client.Poll(0, SelectMode.SelectError)))
                {
                    var buffer = new byte[1];
                    var count = client.Client.Receive(buffer, SocketFlags.Peek);

                    return count != 0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}