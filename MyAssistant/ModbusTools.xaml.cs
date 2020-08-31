using HslCommunication.ModBus;
using MyAssistant.ViewModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YCIOT.ModbusPoll.Utils;

namespace MyAssistant
{
    /// <summary>
    /// 解包工具
    /// </summary>
    public partial class ModbusTools : Window
    {

        public ObservableCollection<ModbusViewModel> requestList = new ObservableCollection<ModbusViewModel>();

        public ObservableCollection<ModbusViewModel> responseList = new ObservableCollection<ModbusViewModel>();


        public ModbusTools()
        {
            InitializeComponent();

            this.dgRequest.ItemsSource = requestList;
            this.dgResponse.ItemsSource = responseList;
        }

        private void btnRequestParse_Click(object sender, RoutedEventArgs e)
        {
            requestList.Clear();
            this.dgRequest.Visibility = Visibility.Visible;
            //01 04 00 0E 00 0A 11 CE

            var package = this.txtRequestPackage.Text;

            if (string.IsNullOrEmpty(package))
            {
                return;
            }

            var slices = package.Split(' ');

            var first = slices.First();

            var second = slices[1];

            var data = HexStringToBytes(package);

            requestList.Add(new ModbusViewModel()
            {
                Data = first,
                Description = "Slave address",
                SliceValue = $" 0x{first} ({data[0]})"
            });


            var registType = GetRegistType(byte.Parse(second));

            requestList.Add(new ModbusViewModel()
            {
                Data = second,
                Description = "Function code",
                SliceValue = $" 0x{second} ({data[1]}) - {registType}"
            });


            var v1 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(data, 2);

            requestList.Add(new ModbusViewModel()
            {
                Data = $"{slices[2]} {slices[3]}",
                Description = "Starting address",
                SliceValue = $" 0x{slices[2]}{slices[3]} ({v1})"
            });

            var v2 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(data, 4);

            requestList.Add(new ModbusViewModel()
            {
                Data = $"{slices[4]} {slices[5]}",
                Description = "Quantity",
                SliceValue = $" 0x{slices[4]}{slices[5]} ({v2})"
            });

            var v3 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(data, 6);

            requestList.Add(new ModbusViewModel()
            {
                Data = $"{slices[6]} {slices[7]}",
                Description = "CRC",
                SliceValue = $" 0x{slices[6]}{slices[7]} ({v3})"
            });


            var crc32 = data.Take(data.Length - 2).ToArray().CRC16();
            if (data[data.Length - 2] != crc32[1] || data[data.Length - 1] != crc32[0])
            {
                MessageBox.Show($"crc error, should be : {crc32[1]} {crc32[0]}");
            }

        }

        public string GetRegistType(byte type)
        {
            string name = "";

            if (type == 1)
            {
                name = "读取线圈寄存器";
            }
            if (type == 2)
            {
                name = "读取离散输入寄存器";
            }
            if (type == 3)
            {
                name = "Read Holding Registers";
            }
            if (type == 4)
            {
                name = "Read Input Registers";
            }
            if (type == 5)
            {
                name = "写单个线圈寄存器";
            }
            if (type == 6)
            {
                name = "Write Single Holding Registers";
            }
            if (type == 15)
            {
                name = "写多个线圈寄存器";
            }
            if (type == 16)
            {
                name = "Write Multiple Holding Registers";
            }
            return name;
        }



        public byte[] HexStringToBytes(string hexString)
        {
            string[] array = hexString.Split(' ');

            List<byte> bytes = new List<byte>();

            string[] hex = { "A", "B", "C", "D", "E", "F" };

            foreach (var item in array)
            {
                if (item.StartsWith("0"))
                {
                    var newValue = item.TrimStart('0');

                    if (hex.Contains(newValue))
                    {
                        switch (newValue)
                        {
                            case "A":
                                newValue = "10";
                                break;
                            case "B":
                                newValue = "11";
                                break;
                            case "C":
                                newValue = "12";
                                break;
                            case "D":
                                newValue = "13";
                                break;
                            case "E":
                                newValue = "14";
                                break;
                            case "F":
                                newValue = "15";
                                break;
                        }
                        bytes.Add(byte.Parse(newValue));
                    }
                    else
                    {
                        if (newValue == "") newValue = "0";
                        var value1 = byte.Parse(newValue);

                        if (value1 < 9)
                        {
                            bytes.Add(value1);
                        }
                    }
                }
                else
                {
                    var chars = item.ToCharArray();

                    var first = chars[0].ToString();

                    if (hex.Contains(first))
                    {
                        switch (first)
                        {
                            case "A":
                                first = "10";
                                break;
                            case "B":
                                first = "11";
                                break;
                            case "C":
                                first = "12";
                                break;
                            case "D":
                                first = "13";
                                break;
                            case "E":
                                first = "14";
                                break;
                            case "F":
                                first = "15";
                                break;
                        }
                    }


                    var second = chars[1].ToString();

                    if (hex.Contains(second))
                    {
                        switch (second)
                        {
                            case "A":
                                second = "10";
                                break;
                            case "B":
                                second = "11";
                                break;
                            case "C":
                                second = "12";
                                break;
                            case "D":
                                second = "13";
                                break;
                            case "E":
                                second = "14";
                                break;
                            case "F":
                                second = "15";
                                break;
                        }
                    }

                    var high = byte.Parse(first);
                    var low = byte.Parse(second);

                    var value1 = high << 4 | low;
                    bytes.Add((byte)value1);
                }
            }

            return bytes.ToArray();
        }




        private void btnResPonseParse_Click(object sender, RoutedEventArgs e)
        {
            responseList.Clear();
            this.dgResponse.Visibility = Visibility.Visible;
            //01 04 14 00 00 00 62 00 61 00 95 73 33 00 00 00 00 07 76 00 00 00 00 0D C0

            var package = this.txtResponsePackage.Text;

            if (string.IsNullOrEmpty(package))
            {
                return;
            }

            var slices = package.Split(' ');

            var first = slices.First();

            var second = slices[1];

            var data = HexStringToBytes(package);

            responseList.Add(new ModbusViewModel()
            {
                Data = first,
                Description = "Slave address",
                SliceValue = $" 0x{first} ({data[0]})"
            });


            var registType = GetRegistType(byte.Parse(second));

            responseList.Add(new ModbusViewModel()
            {
                Data = second,
                Description = "Function code",
                SliceValue = $" 0x{second}({data[1]}) - {registType}"
            });


            var v1 = new ModbusRtuOverTcp().ByteTransform.TransByte(data, 2);

            responseList.Add(new ModbusViewModel()
            {
                Data = $"{slices[2]}",
                Description = "Byte count",
                SliceValue = $" 0x{slices[2]} ({v1})"
            });

            //截取数据部分

            StringBuilder sb = new StringBuilder();

            for (int i = 3; i < slices.Length - 2; i++)
            {
                sb.AppendFormat("{0} ", slices[i]);
            }

            var realData = sb.ToString().TrimEnd(' ');

            var overtcp = new ModbusRtuOverTcp();


            StringBuilder sbValues = new StringBuilder();

            for (int i = 3; i < data.Length - 2; i = i + 2)
            {
                var v = overtcp.ByteTransform.TransUInt16(data, i);

                sbValues.AppendFormat("0x{0}{1} ({2}), ", slices[i], slices[i + 1], v);
            }
            var realValues = sbValues.ToString().Trim().TrimEnd(',');

            responseList.Add(new ModbusViewModel()
            {
                Data = $"{realData}",
                Description = "Register value",
                SliceValue = realValues
            });

            var v3 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(data, data.Length - 2);

            responseList.Add(new ModbusViewModel()
            {
                Data = $"{slices[data.Length - 2]} {slices[data.Length - 1]}",
                Description = "CRC",
                SliceValue = $" 0x{slices[data.Length - 2]}{slices[data.Length - 1]}({v3})"
            });


            var crc32 = data.Take(data.Length - 2).ToArray().CRC16();
            if (data[data.Length - 2] != crc32[1] || data[data.Length - 1] != crc32[0])
            {
                MessageBox.Show($"crc error, should be : {crc32[1]} {crc32[0]}");
            }
        }
    }
}
