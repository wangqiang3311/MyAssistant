using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acme.Common.Utils;
using AMWD.Modbus.Common.Util;
using YCIOT.ModbusPoll.AnSen;
using static System.String;

namespace Ansen 
{
     /// <summary>
    /// 启动80服务，用TCP工具连接，可以模拟设备端，可和服务器通讯
    /// </summary>
    //https://github.com/jchristn/TcpTest/blob/master/Server/Metadata.cs
    internal class AnSen
    {
        public static bool RunForever = true;
        public static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static readonly CancellationToken Token = TokenSource.Token;
        public static readonly IPAddress IpAddress = IPAddress.Parse("127.0.0.1");
        private static readonly TcpListener Listener = new TcpListener(IpAddress, 80);
        public static readonly ConcurrentDictionary<string, Metadata> Clients = new ConcurrentDictionary<string, Metadata>();

        public static void Test()
        {
            Listener.Start();
            Task.Run(() => AcceptConnections(Token), Token);

            while (RunForever)
            {
                Console.Write("Command [? for help]: ");
                var userInput = Console.ReadLine();
                if (IsNullOrEmpty(userInput)) continue;

                switch (userInput)
                {
                    case "?":
                        Menu();
                        break;
                    case "q":
                        RunForever = false;
                        break;
                    case "c":
                    case "cls":
                        Console.Clear();
                        break;
                    case "dispose":
                        DisposeServer();
                        break;
                    case "list":
                        ListClients();
                        break;
                    case "send":
                        SendData();
                        break;
                    case "remove":
                        RemoveClient();
                        break;
                }
            }
        }

        private static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("  q              Quit this program");
            Console.WriteLine("  cls            Clear the screen");
            Console.WriteLine("  dispose        Dispose the server");
            Console.WriteLine("  list           List clients");
            Console.WriteLine("  send           Send data to a client");
            Console.WriteLine("  remove         Remove a client");
            Console.WriteLine("");
        }

        private static void DisposeServer()
        {
            try
            {
                if (Clients != null && Clients.Count > 0)
                {
                    foreach (var (key, value) in Clients)
                    {
                        Console.WriteLine("Disconnecting " + key);
                        value.Dispose();
                    }
                }

                TokenSource.Cancel();
                TokenSource.Dispose();

                if (Listener?.Server != null)
                {
                    Listener.Server.Close();
                    Listener.Server.Dispose();
                }

                Listener?.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    Environment.NewLine +
                    "Dispose Exception:" +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine);
            }
        }

        private static void ListClients()
        {
            Console.WriteLine("");
            Console.WriteLine("Clients: " + Clients.Count);

            foreach (var cur in Clients)
                Console.WriteLine("  " + cur.Key);

            Console.WriteLine("");
        }

        private static void SendData()
        {
            ListClients();
            Console.Write("Client: ");
            var key = Console.ReadLine();
            if (IsNullOrEmpty(key)) return;
            var md = Clients[key.Trim()];

            Console.Write("Data: ");

            var data = new byte[] { 0x01, 0x04, 0x00, 0x01, 0x00, 0x0A, 0x94, 0x08 };

            uint address = 14 + 0;
            data[2] = (byte)(address >> 8);
            data[3] = (byte)(address & 0xFF);
            var length = data.Length;
            var crc32 = data.Take(length - 2).ToArray().CRC16();

            data[length - 2] = crc32[0];

            data[length - 1] = crc32[1];

            //var dataBytes = Encoding.UTF8.GetBytes(data);

            lock (md.SendLock)
            {
                md.NetworkStream.Write(data, 0, data.Length);
                md.NetworkStream.Flush();
            }
        }

        private static void RemoveClient()
        {
            ListClients();
            Console.Write("Client: ");
            var key = Console.ReadLine();
            if (IsNullOrEmpty(key)) return;
            var md = Clients[key];

            Clients.TryRemove(md.TcpClient.Client.RemoteEndPoint.ToString(), out _);
            md.Dispose();
        }

        private static async Task AcceptConnections(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var client = await Listener.AcceptTcpClientAsync();
                var md = new Metadata(client);
                Clients.TryAdd(client.Client.RemoteEndPoint.ToString(), md);
                await Task.Run(() => DataReceiver(md), md.Token);
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

        private static async Task DataReceiver(Metadata md)
        {
            var header = "[" + md.TcpClient.Client.RemoteEndPoint.ToString() + "]";
            Console.WriteLine(header + " data receiver started");

            try
            {
                while (true)
                {
                    if (!IsClientConnected(md.TcpClient))
                    {
                        Console.WriteLine(header + " client no longer connected");
                        break;
                    }

                    if (Token.IsCancellationRequested)
                    {
                        Console.WriteLine(header + " cancellation requested");
                        break;
                    }

                    var data = await DataReadAsync(md.TcpClient, Token);
                    if (data == null || data.Length < 1)
                    {
                        await Task.Delay(30);
                        continue;
                    }

                    Console.WriteLine(data + ": " + BytesToHexString(data));
                    var length = data.Length;

                    if (data.Length == 3)
                    {
                        var deviceId = data[2];

                        switch (data[1])
                        {
                            case 0xC7:
                                Console.WriteLine($"收到设备[0x{deviceId}]注册包");
                                break;
                            case 0xC8:
                                Console.WriteLine($"收到设备[0x{deviceId}]心跳包");
                                break;
                        }
                    }

                    if (length == 25)
                    {
                        var crc32 = data.Take(length - 2).ToArray().CRC16();

                        if (data[length - 2] == crc32[0] && data[length - 1] == crc32[1])
                        {

                            //DtuId：212
                            //WellId：3
                            //配注仪状态: 0
                            Console.WriteLine($"配注仪状态 : {data[4]}");
                            //设定流量回读: 0.17
                            var t = BCDUtils.BCDToUshort((ushort) (data[5] << 8 | data[6]));
                            Console.WriteLine($"设定流量回读 : {t / 100.0}");

                            //瞬时流量: 0.15
                            t = BCDUtils.BCDToUshort((ushort)(data[7] << 8 | data[8]));
                            Console.WriteLine($"瞬时流量 : { t / 100.0}");

                            //累计流量: 607.26
                            var t1 = BCDUtils.BCDToUshort((ushort)(data[9] << 8 | data[10]));
                            var t2 = BCDUtils.BCDToUshort((ushort)(data[11] << 8 | data[12]));
                            Console.WriteLine($"累计流量 : {(t1 *10000 +  t2) / 100.0}");
                            //扩展: 0.0
                            //水井压力: 7.35
                            t = BCDUtils.BCDToUshort((ushort)(data[17] << 8 | data[18]));
                            Console.WriteLine($"水井压力 : {t / 100.0}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    Environment.NewLine +
                    header +
                    " DataReceiver Exception: " +
                    Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine);
            }

            Console.WriteLine(header + " data receiver terminating");

            Clients.TryRemove(md.TcpClient.Client.RemoteEndPoint.ToString(), out _);
            md.Dispose();
        }

        private static async Task<byte[]> DataReadAsync(TcpClient client, CancellationToken token)
        {
            if (token.IsCancellationRequested) throw new OperationCanceledException();

            var stream = client.GetStream();
            if (!stream.CanRead) return null;
            if (!stream.DataAvailable) return null;

            var buffer = new byte[1024];

            await using var ms = new MemoryStream();
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                if (read > 0)
                {
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
                    return client.Client.Receive(buffer, SocketFlags.Peek) != 0;
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
    }
}