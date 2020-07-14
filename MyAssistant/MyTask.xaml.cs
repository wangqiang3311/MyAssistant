using MyAssistant.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyAssistant
{
     /// <summary>
     /// MyTask.xaml 的交互逻辑
     /// </summary>
     public partial class MyTask : Window
     {
          public ObservableCollection<TaskViewModel> tasklist = new ObservableCollection<TaskViewModel>();
         
          TaskBar bar = new TaskBar();

          public MyTask()
          {
               InitializeComponent();

               #region  创建状态栏图标
               bar.Left = SystemParameters.WorkArea.Size.Width - bar.Width;
               bar.Top = SystemParameters.WorkArea.Size.Height - bar.Height;
               bar.txtMessage.Text = "我的任务已启动";
               bar.Show();
               bar.Topmost = true;
               bar.CloseMessage(10);

               #endregion

               this.ItemTask.ItemsSource = tasklist;

               tasklist.Add(new TaskViewModel()
               {
                    Id = 1,
                    DateCreate = DateTime.Now,
                    Description="my first Task",
                    IsRemind=true,
                    Name="完成演讲ppt",
                    PlanFinishDate=DateTime.Now
               }); ;
          }
          private void txtAddTask_Click(object sender, RoutedEventArgs e)
          {
               AddTask t = new AddTask();
               t.owner = this;
               t.ShowDialog();
          }

          private void Window_Closed(object sender, EventArgs e)
          {
               bar?.Close();
          }
     }
}
