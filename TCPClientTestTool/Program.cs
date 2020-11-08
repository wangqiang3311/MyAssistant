using Acme.Common;
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
            int interval = 10;

            for (int i = 0; i < clientCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var j = i;
                    Dowork(ip, port.ToString(), interval, j);
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


        private static void Dowork(string ip, string port, int interval, int index)
        {
            string connectionErr = "";
            Sender sender = new Sender();

            var isConnected = sender.Connect(ip, int.Parse(port), out connectionErr);

            if (isConnected)
            {
                Receive(sender);
                Send(sender);
            }
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
            while (true)
            {
                if (!DataQueue.TryDequeue(out var data))
                {
                    Thread.Sleep(20);
                    continue;
                }

                //some condition was matched, send data

                var isSended = sender.SendMessage(data);

                if (isSended)
                {
                    Logger.Info("send success");
                }
            }
        }
    }
}
