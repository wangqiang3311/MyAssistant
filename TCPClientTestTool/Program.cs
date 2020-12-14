using Acme.Common;
using Acme.Common.Utils;
using ModbusCommon;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClientTestTool
{
    public class Program
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly ConcurrentQueue<byte[]> DataQueue = new ConcurrentQueue<byte[]>();


        public static void Main(string[] args)
        {
            Logger.Info("tcp client start running");

            string ip = ConfigurationManager.AppSettings["LocalIP"];
            int port = int.Parse(ConfigurationManager.AppSettings["TcpPort.Many"]);

            int clientCount = 1;
            int interval = 5000;

            for (int i = 0; i < clientCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var j = i;
                    //to do   linkId从配置表中读取
                    Dowork(ip, port.ToString(), interval, 731);
                });
            }

            while (true)
            {
                try
                {
                    Console.WriteLine("please input your response package");
                    var responsePackage = Console.ReadLine();

                    if (responsePackage != "")
                    {
                        var data = StringHelper.HexStringToBytes(responsePackage);

                        DataQueue.Enqueue(data);
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


        private static void Dowork(string ip, string port, int interval, short linkId)
        {
            string connectionErr = "";
            Sender sender = new Sender();

            var isConnected = sender.Connect(ip, int.Parse(port), out connectionErr);

            if (isConnected)
            {
                //发送注册包
                Regist(sender, linkId);

                //每隔3s发送心跳包
                SendBeatHeart(sender, interval, linkId);

                Receive(sender);
                Send(sender);
            }
        }

        private static void SendBeatHeart(Sender sender, int interval, short linkId)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        List<byte> bytes = new List<byte>();
                        bytes.Add(0x00);
                        bytes.Add(0xC8);
                        bytes.AddRange(DataTranse.ShortToByte(linkId));

                        bool isSended = sender.SendMessage(bytes.ToArray());

                        Thread.Sleep(interval);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"发送心跳包失败:" + ex.Message);
                    }
                }
            });
        }

        private static void Regist(Sender sender, short linkId)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(0x00);
            bytes.Add(0xC7);
            bytes.AddRange(DataTranse.ShortToByte(linkId));

            bool isSended = sender.SendMessage(bytes.ToArray());
        }


        /// <summary>
        /// 模拟设备接收命令并返回数据
        /// </summary>
        /// <param name="sender"></param>
        public static void Receive(Sender sender)
        {
            Task.Run(function: async () =>
            {
                while (true)
                {
                    try
                    {
                        var bytes = await sender.ReceiveMessage();

                        if (bytes == null)
                        {
                            Thread.Sleep(20);
                            continue;
                        }

                        var receivedata = StringHelper.BytesToHexString(bytes);

                        DataQueue.Enqueue(bytes);

                        Logger.Info("received data：" + receivedata);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("接收数据出错：" + ex.Message);
                    }
                }
            });
        }

        public static void Send(Sender sender)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (!DataQueue.TryDequeue(out var data))
                    {
                        Thread.Sleep(20);
                        continue;
                    }
                    //some condition was matched, send data
                    //判断井状态
                    if (StringHelper.BytesToHexString(data).StartsWith("01 03 10 07 00 01 31 0B"))
                    {
                        var sendData = StringHelper.HexStringToBytes("01 03 02 00 01 79 84");

                        var isSended = sender.SendMessage(sendData);

                        if (isSended)
                        {
                            Logger.Info("send well status response success");
                        }
                    }

                    //读取贵隆2000电表
                    if (StringHelper.BytesToHexString(data).StartsWith("01 03 10 19 00 07 D1 0F"))
                    {
                        var sendData = StringHelper.HexStringToBytes("01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32");

                        var isSended = sender.SendMessage(sendData);

                        if (isSended)
                        {
                            Logger.Info("send DB success");
                        }
                    }

                    if(StringHelper.BytesToHexString(data).StartsWith("01 03 10 2D 00 04 D0 C0"))
                    {
                        var sendData = StringHelper.HexStringToBytes("01 03 08 33 56 A4 08 E3 00 07 00 2D 82");

                        var isSended = sender.SendMessage(sendData);

                        if (isSended)
                        {
                            Logger.Info("send DB success");
                        }
                    }
                }
            });
        }
    }
}
