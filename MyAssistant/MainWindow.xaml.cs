using MyAssistant;
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
        public MainWindow()
        {
            InitializeComponent();
            MyAssistants.Patch();

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
    }
}
