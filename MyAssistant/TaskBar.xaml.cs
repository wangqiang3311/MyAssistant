using System;
using System.Collections.Generic;
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

namespace MyAssistant
{
     /// <summary>
     /// TaskBar.xaml 的交互逻辑
     /// </summary>
     public partial class TaskBar : Window
     {
          public TaskBar()
          {
               InitializeComponent();
          }
          public void CloseMessage(int seconds = 3)
          {
               Task.Run(function: async () =>
               {
                    await Task.Delay(seconds * 1000);

                    await this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                             (ThreadStart)delegate ()
                             {
                                  this.Close();
                             });
               });
          }
     }

}
