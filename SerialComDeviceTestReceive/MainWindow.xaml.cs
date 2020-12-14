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

namespace SerialComDeviceTestReceive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private GodSerialPort _serialPort;
        private bool _isOpened;


        private void receiveWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialPort =
                  new GodSerialPort("COM8", 9600, Parity.None, 8, StopBits.One)
                  {
                      DataFormat = SerialPortDataFormat.Hex,
                      RtsEnable = true,
                      DtrEnable = true
                  };

                Open();

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var data = _serialPort.ReadString();

                        this.Dispatcher.Invoke(() =>
                        {
                            if (!string.IsNullOrEmpty(data))
                                this.rtxReceiveData.AppendText(data + "\n");
                        });

                        Thread.Sleep(3000);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("串口数据接收错误：" + ex.ToString());
            }
        }

        public async void Open()
        {
            _isOpened = _serialPort.Open();
            if (_isOpened == false)
            {
                await Task.Delay(2000);
                _isOpened = _serialPort.Open();
            }
        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.Dispatcher.Invoke(() =>
            {
                rtxReceiveData.AppendText(sp.ReadExisting());
            });
        }


        private void receiveWindow_Closed(object sender, EventArgs e)
        {
            _serialPort.Close();
        }
    }
}
