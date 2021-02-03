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
        public static readonly ConcurrentQueue<byte[]> ReceiveDataQueue = new ConcurrentQueue<byte[]>();

        public static RunState runstate;

        public static void Main(string[] args)
        {
            Logger.Info("tcp client start running");

            string ip = ConfigurationManager.AppSettings["LocalIP"];
            int port = int.Parse(ConfigurationManager.AppSettings["TcpPort.Many"]);

            int clientCount = 1;

            //间隔5分钟
            int interval = 60 * 5 * 1000;

            for (int i = 0; i < clientCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var j = i;
                    Dowork(ip, port.ToString(), interval);
                });
            }

            var j = 1;

            runstate = RunState.Init;

            while (true)
            {
                try
                {
                    if (runstate == RunState.Sleep)
                    {
                        j = 1;

                        Thread.Sleep(interval);

                        runstate = RunState.Init;
                    }

                    if (runstate == RunState.Init)
                    {
                        int packageZize = 5;

                        for (int i = 1; i <= packageZize; i++)
                        {
                            var responsePackage = MakePackage(i, packageZize);

                            if (responsePackage != "")
                            {
                                Console.WriteLine("report package:" + responsePackage);
                                var data = StringHelper.HexStringToBytes(responsePackage);
                                DataQueue.Enqueue(data);
                            }
                        }

                        runstate = RunState.ReadyReport;
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    break;
                }
            }
            Console.Read();
        }

        private static string MakePackage(int seq, int packageZize = 5)
        {
            //数据帧头
            string frameHead = "7E";
            //数据帧长度
            string frameLength = "00 1E";
            //仪表编号，ASCII码，12字节
            string no = "44 59 30 31 30 37 32 32 30 30 30 31";

            //功能码，上报
            string function = "01";

            //内容N个字节

            string hasNext = seq == packageZize ? "00" : "01";
            string contentHead = $"00 0{seq} {hasNext} 01 00 00 09 00 02 09 08 04 05 07 08 06 05 01";

            string content = "00 02 21 01 30 11 24 00 01 00 01 00 09 00 10 21 01 30 11 24 00 01 00 01 00 09 00 10";

            //校验位 crc 2个字节
            string crc = "44 31";

            string package = $"{frameHead} {frameLength} {no} {function} {contentHead} {content} {crc}";

            return package;
        }


        private static string MakeFinishPackage()
        {
            //数据帧头
            string frameHead = "7E";
            //数据帧长度
            string frameLength = "00 1E";
            //仪表编号，ASCII码，12字节  DY0107220001
            string no = "44 59 30 31 30 37 32 32 30 30 30 31";

            //功能码，结束反馈帧
            string function = "06";

            //内容为空
            //校验位 crc 2个字节
            string crc = "44 31";

            string package = $"{frameHead} {frameLength} {no} {function} {crc}";

            return package;
        }

        private static void Dowork(string ip, string port, int interval)
        {
            string connectionErr = "";
            Sender sender = new Sender();

            var isConnected = sender.Connect(ip, int.Parse(port), out connectionErr);

            if (isConnected)
            {
                //每隔1分钟上报
                Send(sender, interval);
                Receive(sender);
            }
        }

        /// <summary>
        /// 模拟设备接收反馈
        /// </summary>
        /// <param name="sender"></param>
        public static void Receive(Sender sender)
        {
            var task = Task.Factory.StartNew(function: async () =>
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

                          ReceiveDataQueue.Enqueue(bytes);

                          Logger.Info("received data：" + receivedata);

                          Thread.Sleep(100);
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine("接收数据出错：" + ex.Message);
                      }
                  }
              }, TaskCreationOptions.LongRunning);
        }


        public static void Send(Sender sender, int interval)
        {
            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (runstate == RunState.Sleep)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    //收到反馈包或者结束包
                    if (ReceiveDataQueue.Count > 0)
                    {
                        if (ReceiveDataQueue.TryDequeue(out var receiveData))
                        {
                            //判断是否收到结束包
                            if (receiveData.Length > 15)
                            {
                                //取功能码，判断是否为结束帧
                                var functionCodeFeadBack = receiveData[14];
                                var functionCode = receiveData[15];

                                if (functionCode == 5 && runstate == RunState.FeedBack)
                                {
                                    runstate = RunState.ReceivedFinish;
                                    var finishPackage = MakeFinishPackage();

                                    if (finishPackage != "")
                                    {
                                        Console.WriteLine("finish package:" + finishPackage);
                                        var data = StringHelper.HexStringToBytes(finishPackage);
                                        DataQueue.Enqueue(data);
                                    }
                                }
                                else if (functionCodeFeadBack == 2)
                                {
                                    runstate = RunState.FeedBack;
                                }
                                else
                                {
                                    runstate = RunState.Sleep;
                                }
                            }
                        }
                    }

                    //如果是首次上报
                    if (runstate == RunState.ReadyReport)
                    {
                        if (!DataQueue.TryDequeue(out var data))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        var isSended = sender.SendMessage(data);

                        if (isSended == true)
                        {
                            Logger.Info("first report package");
                        }
                    }
                    if (runstate == RunState.FeedBack)
                    {
                        if (!DataQueue.TryDequeue(out var data))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        var isSended = sender.SendMessage(data);

                        if (isSended == true)
                        {
                            Logger.Info("next report package");
                        }
                    }

                    if (runstate == RunState.ReceivedFinish)
                    {
                        if (!DataQueue.TryDequeue(out var data))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        var isSended = sender.SendMessage(data);

                        if (isSended == true)
                        {
                            runstate = RunState.Sleep;
                            Logger.Info("Finish FeedBack package");
                        }
                    }

                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
    /// <summary>
    /// 运行阶段
    /// </summary>
    public enum RunState
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        Init,
        /// <summary>
        /// 准备上报
        /// </summary>
        ReadyReport,
        /// <summary>
        /// 修改
        /// </summary>
        Modify,
        /// <summary>
        /// 反馈
        /// </summary>
        FeedBack,
        Other,
        /// <summary>
        /// 接收完成
        /// </summary>
        ReceivedFinish,
        /// <summary>
        /// 休息
        /// </summary>
        Sleep
    }
}
