using MyAssistant;
using ServiceStack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MyAssistant
{
    public partial class MainWindow : Window
    {
       private readonly IDbConnectionFactory _factory;
        public MainWindow(IDbConnectionFactory factory)
        {
            InitializeComponent();
            MyAssistants.Patch();

            _factory = factory;

            EventManager.RegisterClassHandler(typeof(ListBoxItem),
    ListBoxItem.MouseLeftButtonDownEvent,
    new RoutedEventHandler(this.OnMouseLeftButtonDown));

        }

        private void OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (item != null)
            {
                if (item.Name == "publishManage")
                {
                    publishManage_MouseDoubleClick(null, null);
                }
                if (item.Name == "MyTask")
                {
                    MyTask_MouseDoubleClick(null, null);
                }
                if (item.Name == "modbusPackage")
                {
                    modbusPackage_MouseDoubleClick(null, null);
                }

                if (item.Name == "tcpTool")
                {
                    TCPTools_MouseDoubleClick (null, null);
                }

                if (item.Name== "doTest")
                {
                    doTest_MouseDoubleClick(null, null);
                }
                if(item.Name== "doReceive")
                {
                    doReceive_MouseDoubleClick(null, null);
                }
                if(item.Name== "waterUnPackage")
                {
                    waterUnPackage_MouseDoubleClick(null, null);
                }
                if (item.Name == "doExport")
                {
                    doExport_MouseDoubleClick(null, null);
                }
            }
        }

        private void MyTask_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MyTask t = new MyTask();
            t.Owner = this;
            t.ShowDialog();
        }

        private void publishManage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UpdateManage w = new UpdateManage();
            w.Owner = this;
            w.ShowDialog();
        }

        private void modbusPackage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ModbusTools w = new ModbusTools();
            w.Owner = this;
            w.ShowDialog();
        }

        private void TCPTools_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TCPTools w = new TCPTools();
            w.Owner = this;
            w.ShowDialog();
        }

        /// <summary>
        /// 包发送管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void doTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TestTools w = new TestTools();
            w.Owner = this;
            w.ShowDialog();
        }

        private void doReceive_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ReceiveTools w = new  ReceiveTools();
            w.Owner = this;
            w.ShowDialog();
        }

        private void doExport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExportTools w = new ExportTools();
            w.Owner = this;
            w.ShowDialog();
        }

        private void waterUnPackage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UnPackWater w = new UnPackWater();
            w.Owner = this;
            w.ShowDialog();
        }
    }
}
