using System;
using System.Collections.Generic;
using System.Text;
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
     public partial class AddTask : Window
     {
          public AddTask()
          {
               InitializeComponent();

               this.DataContext = new ViewModel.TaskViewModel()
               {
                    DateCreate = DateTime.Now,
                    PlanFinishDate = DateTime.Now,
                    IsRemind = true,
               };
          }

          public Window owner;
          private void btnOk_Click(object sender, RoutedEventArgs e)
          {
               var t = owner as MyTask;
               t.tasklist.Add(this.DataContext as ViewModel.TaskViewModel);

               //保存到数据库


               this.Close();
          }
     }
}
