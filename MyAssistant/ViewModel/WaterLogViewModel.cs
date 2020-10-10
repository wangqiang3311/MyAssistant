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
    public class WaterLogViewModel : INotifyPropertyChanged
    {

        private string from;

        public string From
        {
            get
            {
                return from;
            }
            set
            {
                if (from == value) return;

                from = value;
                Notify("From");
            }
        }

        private string to;

        public string To
        {
            get
            {
                return to;
            }
            set
            {
                if (to == value) return;

                to = value;
                Notify("To");
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

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime DateCreate { set; get; }


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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
