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
     public class TaskViewModel : INotifyPropertyChanged
     {

          public int Id { set; get; }

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

          private bool isRemind;

          public bool IsRemind
          {
               get
               {
                    return isRemind;
               }
               set
               {
                    if (isRemind == value) return;

                    isRemind = value;
                    Notify("IsRemind");
               }
          }

          /// <summary>
          /// 创建日期
          /// </summary>
          public DateTime DateCreate { set; get; }

          /// <summary>
          /// 计划完成时间
          /// </summary>
          public DateTime PlanFinishDate { set; get; }

          /// <summary>
          /// 实际完成时间
          /// </summary>
          public DateTime ActualFinishDate { set; get; }


          private string dateCreateStr;

          /// <summary>
          ///创建时间格式化 
          /// </summary>
          public string DateCreateStr
          {
               get
               {
                    dateCreateStr = DateCreate.ToString("yyyy-MM-dd HH:mm:ss");
                    return dateCreateStr;
               }
               set
               {
                    if (dateCreateStr == value) return;

                    dateCreateStr = value;
                    if (!string.IsNullOrEmpty(dateCreateStr))
                    {
                         DateCreate = Convert.ToDateTime(dateCreateStr);
                         Notify("DateCreateStr");
                    }
               }
          }


          private string planFinishDateStr;

          public string PlanFinishDateStr
          {
               get
               {
                    planFinishDateStr = PlanFinishDate.ToString("yyyy-MM-dd HH:mm:ss");
                    return planFinishDateStr;
               }
               set
               {
                    if (planFinishDateStr == value) return;

                    planFinishDateStr = value;

                    if (!string.IsNullOrEmpty(planFinishDateStr))
                    {
                         PlanFinishDate = Convert.ToDateTime(planFinishDateStr);
                         Notify("PlanFinishDateStr");
                    }
               }
          }

          private string actualFinishDateStr;
          public string ActualFinishDateStr
          {
               get
               {
                    actualFinishDateStr = ActualFinishDate.ToString("yyyy-MM-dd HH:mm:ss");
                    return actualFinishDateStr;
               }
               set
               {
                    if (actualFinishDateStr == value) return;

                    actualFinishDateStr = value;
                    if (!string.IsNullOrEmpty(actualFinishDateStr))
                    {
                         ActualFinishDate = Convert.ToDateTime(actualFinishDateStr);
                         Notify("ActualFinishDateStr");
                    }
               }
          }


          public event PropertyChangedEventHandler PropertyChanged;

          protected void Notify(string propName)
          {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
          }
     }
}
