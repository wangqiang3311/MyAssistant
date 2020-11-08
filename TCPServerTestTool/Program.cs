using Acme.Common;
using Microsoft.Extensions.Logging;
using ModbusCommon;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServerTestTool
{
    public class Program
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("tcp server program on running... ");

            Init();

            while (true)
            {
                try
                {
                    Console.WriteLine("please input your request package");
                    var requestPackage = Console.ReadLine();

                    if (requestPackage != "")
                    {
                        var bytes = StringHelper.HexStringToBytes(requestPackage);
                        DataQueue.Enqueue(bytes);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    break;
                }
            }
            Console.Read();
        }

        private static TcpListener ManyListener;

        public static readonly ConcurrentDictionary<string, ManyMetadata> Clients = new ConcurrentDictionary<string, ManyMetadata>();
        public static readonly ConcurrentQueue<byte[]> DataQueue = new ConcurrentQueue<byte[]>();

        public static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static readonly CancellationToken Token = TokenSource.Token;

        private static void Init()
        {
            string ip = ConfigurationManager.AppSettings["LocalIP"];
            int port = int.Parse(ConfigurationManager.AppSettings["TcpPort.Many"]);

            var ipAddress = IPAddress.Parse(ip);
            ManyListener = new TcpListener(ipAddress, port);
            ManyListener.Start();

            Logger.Info("tcp server on working...");

            #region  build connection and receive data

            Task.Run(() => ManyAcceptConnections(Token), Token);

            #endregion

            #region 1->M send data
            Task.Run(() =>
             {
                 while (true)
                 {
                     try
                     {
                         if (!DataQueue.TryDequeue(out var data))
                         {
                             Thread.Sleep(20);
                             continue;
                         };

                         foreach (var cur in Clients)
                         {
                             var key = cur.Value.TcpClient.Client.RemoteEndPoint.ToString();
                             if (!Clients.TryGetValue(key, out var value)) continue;

                             value.NetworkStream.Write(data, 0, data.Length);
                             value.NetworkStream.Flush();

                             Logger.Info($"[1->M]----{StringHelper.BytesToHexString(data)}----");
                         }
                     }
                     catch (Exception e)
                     {
                         Logger.Error("send error：" + e.Message);
                     }
                 }
             });

            #endregion
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
                    Clients.TryAdd(client.Client.RemoteEndPoint.ToString(), md);

                    Logger.Info($"设备连接数：{Clients.Count}");

#pragma warning disable 4014
                    Task.Run(() => ManyDataReceiver(md), md.Token);
#pragma warning restore 4014

                }
                catch (Exception e)
                {
                    Logger.Info(e.Message);
                }
            }
        }

        private static async Task ManyDataReceiver(ManyMetadata md)
        {
            var header = "[" + md.TcpClient.Client.RemoteEndPoint.ToString() + "]";
            Logger.Info($"{header} many data receiver started");

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
                        Logger.Info(header + " cancellation requested");
                        break;
                    }

                    var data = await DataReadAsync(md.TcpClient, Token);

                    if (data == null || data.Length < 1)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    Logger.Info($"[M-1][{md.TcpClient.Client.RemoteEndPoint}]:{StringHelper.BytesToHexString(data)}");


                    var length = data.Length;

                    var linkId = 0;

                    if (data.Length > 3)
                    {
                        var linkIdHigh = data[2];
                        var linkIdLow = data[3];

                        linkId = (linkIdHigh << 8) | (linkIdLow);

                        switch (data[1])
                        {
                            case 0xC7:

                                Logger.Info($"收到连接[4][{Clients.Count}][{linkId}]注册包");

                                break;

                            case 0xC8:

                                Logger.Info($"收到连接[4][{Clients.Count}][{linkId}]心跳包");

                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Info(
                     Environment.NewLine +
                     header +
                     " ManyDataReceiver Exception: " +
                     Environment.NewLine +
                     e.ToString() +
                     Environment.NewLine +
                     e.StackTrace
                     );
            }

            Logger.Info(header + " data receiver terminating");

            Clients.TryRemove(md.TcpClient.Client.RemoteEndPoint.ToString(), out _);
            md.Dispose();
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

        private static async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        {
            if (token.IsCancellationRequested) throw new OperationCanceledException();

            var stream = client.GetStream();
            if (!stream.CanRead) return null;
            if (!stream.DataAvailable) return null;

            var buffer = new byte[1024];

            await using var ms = new MemoryStream();

            var timeout = 3000;
            DateTime start = DateTime.Now;

            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                if (read > 0)
                {
                    DateTime end = DateTime.Now;
                    if (end.Subtract(start).TotalMilliseconds > timeout)
                    {
                        Logger.Info("receive data timeout");
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
    }
}
