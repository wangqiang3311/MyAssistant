using Acme.Common;
using GodSharp.SerialPort;
using GodSharp.SerialPort.Enums;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
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

namespace SerialComDeviceTestSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GodSerialPort _serialPort;
        public MainWindow()
        {
            InitializeComponent();

            //To Do:从配置文件中获取com口、波特率等串口参数

            _serialPort =
            new GodSerialPort("COM8", 9600, Parity.None, 8, StopBits.One)
            {
                DataFormat = SerialPortDataFormat.Hex,
                RtsEnable = true,
                DtrEnable = true
            };
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

            _serialPort.Write(datas);
        }

        int times = 6;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _serialPort.Open();

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


            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var data = _serialPort.ReadString();

                    this.Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(data))
                            content.Text += data + "\n";
                    });

                    Thread.Sleep(3000);
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _serialPort.Close();
        }
    }
}
