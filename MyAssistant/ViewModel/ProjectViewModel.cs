using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace MyAssistant.ViewModel
{
     public class ProjectViewModel : INotifyPropertyChanged
     {

          public int Id { set; get; }

          /// <summary>
          /// 1：延长 2：安森
          /// </summary>
          public int TypeId { set; get; }

          private string name;

          public string Name
          {
               get
               {
                    return name;
               }
               set
               {
                    if (name == value) return;

                    name = value;
                    Notify("Name");
               }
          }

          private bool isChecked = true;

          public bool IsChecked
          {
               get
               {
                    return isChecked;
               }
               set
               {
                    if (isChecked == value) return;

                    isChecked = value;
                    Notify("IsChecked");
               }
          }

          private bool isEnable;

          public bool IsEnable
          {
               get
               {
                    return isEnable;
               }
               set
               {
                    if (isEnable == value) return;

                    isEnable = value;
                    Notify("IsEnable");
               }
          }

          private bool isStarted;

          public bool IsStarted
          {
               get
               {
                    return isStarted;
               }
               set
               {
                    if (isStarted == value) return;

                    isStarted = value;
                    Notify("IsStarted");
               }
          }


          private string description;

          public string Description
          {
               get
               {
                    return description;
               }
               set
               {
                    if (description == value) return;

                    description = value;
                    Notify("Description");
               }
          }

          public event PropertyChangedEventHandler PropertyChanged;

          protected void Notify(string propName)
          {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
          }
     }
}
