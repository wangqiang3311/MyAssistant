using HslCommunication.ModBus;
using ModbusCommon;
using MyAssistant.ViewModel;
using NLog;
using ServiceStack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using YCIOT.ModbusPoll.Utils;

namespace MyAssistant
{
    /// <summary>
    /// TCP工具
    /// </summary>
    public partial class TCPTools : Window
    {

        public static ObservableCollection<TCPViewModel> responseList = new ObservableCollection<TCPViewModel>();


        public TCPTools()
        {
            InitializeComponent();

            this.dgResponse.ItemsSource = responseList;
        }

        private void btnCreateServer_Click(object sender, RoutedEventArgs e)
        {
            responseList.Clear();
            BuildServer(this.txtIP.Text, int.Parse(this.txtPort.Text));
        }

        private void btnCreateClient_Click(object sender, RoutedEventArgs e)
        {
            responseList.Clear();

            string ip = this.txtIP.Text;
            string port = this.txtPort.Text;

            Task.Factory.StartNew(() =>
            {
                ConnectServer(ip, int.Parse(port));
            });
        }

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static TcpListener ManyListener;

        public static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static readonly CancellationToken Token = TokenSource.Token;


        private void BuildServer(string ip, int port)
        {
            var ipAddress = IPAddress.Parse(ip);
            ManyListener = new TcpListener(ipAddress, port);
            ManyListener.Start();

            Logger.Info("tcp server on working...");

            #region  build connection

            Task.Run(() => ManyAcceptConnections(Token), Token);

            #endregion
        }

        private static  Sender sender = new Sender();

        private void ConnectServer(string ip, int port)
        {
            string connectionErr = "";

            var isConnected = sender.Connect(ip, port, out connectionErr);

            string message = "";
            if (isConnected)
            {
                message = "连接成功";
                Logger.Info(message);
                sender.SendMessage(Encoding.UTF8.GetBytes(message));
            }
            else
            {
                message = "连接失败";
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                responseList.Add(new TCPViewModel()
                {
                    Data = message,
                    IsOk = message == "连接成功" ? true : false
                });
            });
        }

        public static readonly ConcurrentDictionary<string, TcpClient> Clients = new ConcurrentDictionary<string, TcpClient>();


        private async Task ManyAcceptConnections(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await ManyListener.AcceptTcpClientAsync();
                    Logger.Warn($"设备已连接,IP:{client.Client.RemoteEndPoint}");

                    Clients.TryAdd(client.Client.RemoteEndPoint.ToString(), client);

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        responseList.Add(new TCPViewModel()
                        {
                            Data = client.Client.RemoteEndPoint.ToString(),
                            IsOk = true
                        });
                    });
                }
                catch (Exception e)
                {
                    Logger.Info(e.Message);
                }
            }
        }
    }
}
