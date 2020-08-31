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
    public class ModbusViewModel : INotifyPropertyChanged
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


        private string sliceValue;

        public string SliceValue
        {
            get
            {
                return sliceValue;
            }
            set
            {
                if (sliceValue == value) return;

                sliceValue = value;
                Notify("SliceValue");
            }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
