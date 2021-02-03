using Acme.Common;
using GodSharp.SerialPort;
using GodSharp.SerialPort.Enums;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YCIOT.ModbusPoll.Utils;

namespace SerialComDeviceTestSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GodSerialPort _serialPort;
        public MainWindow()
        {
            InitializeComponent();

            //To Do:从配置文件中获取com口、波特率等串口参数

            //一个大坑，没有这个前面的appSettings出不来
            var assembliesWithServices = new Assembly[1];
            assembliesWithServices[0] = typeof(AppHost).Assembly;
            var appHost = new AppHost("AppHost", assembliesWithServices);

            IAppSettings appSettings = new AppSettings();

            var com = appSettings.Get<string>("COM");
            var rate = appSettings.Get<int>("Rate");
        }

        private void testSend_Click(object sender, RoutedEventArgs e)
        {
            //写入贵隆2000电表

            var hex = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";
            hex = "01 03 00 02 00 02 65 CB";
            var sendData = StringHelper.HexStringToBytes(hex);
            SendData(sendData);
        }

        private void SendData(byte[] datas)
        {
            this.Dispatcher.Invoke(() =>
            {
                content.Text = StringHelper.BytesToHexString(datas);
            });

            if (_serialPort.IsOpen)
                _serialPort.Write(datas);
            else
            {
                Console.WriteLine("串口未打开");
            }
        }

        int times = 6;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Random r = new Random();

            var hex1 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            var hex2 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            var hex3 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            var hex4 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            var hex5 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            var hex6 = "01 03 0E 08 DE 08 A4 08 E3 00 07 00 0C 00 0A 00 00 23 32";

            List<string> hexList = new List<string>()
            {
                hex1,hex2, hex3,hex4, hex5,hex6
            };

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);

                while (true)
                {
                    if (times < 0) break;

                    var n = r.Next(0, 5);

                    var data = StringHelper.HexStringToBytes(hexList[n]);

                    //SendData(data);

                    times--;
                    Thread.Sleep(10000);
                }

            });

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                _serialPort?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("串口关闭失败：" + ex.Message);
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            _serialPort = new GodSerialPort(txtCOM.Text, Convert.ToInt32(txtRate.Text), Parity.None, 8, StopBits.One)
            {
                DataFormat = SerialPortDataFormat.Hex,
                RtsEnable = true,
                DtrEnable = true
            };

            _serialPort.Open();

            if (!_serialPort.IsOpen)
            {
                Console.WriteLine("串口打开失败");
            }
            else
            {
                int interval = Convert.ToInt32(txtReceiveInterval.Text);

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var data = _serialPort?.ReadString();

                        this.Dispatcher.Invoke(() =>
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                if (data.StartsWith("00 C7") || data.StartsWith("00 C8"))
                                {
                                    Console.WriteLine("收到注册包或心跳包" + data);
                                }
                                else
                                {
                                    content.Text = data + "\r\n";
                                }
                            }
                        });


                        Thread.Sleep(5000);
                    }
                });
            }
        }
    }
}
