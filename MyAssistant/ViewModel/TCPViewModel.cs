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
    public class TCPViewModel : INotifyPropertyChanged
    {
        private string data;

        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                if (data == value) return;

                data = value;
                Notify("Data");
            }
        }

        private bool isOk;

        public bool IsOk
        {
            get
            {
                return isOk;
            }
            set
            {
                if (isOk == value) return;

                isOk = value;
                Notify("IsOk");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
