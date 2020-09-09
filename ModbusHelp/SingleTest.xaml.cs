using ModbusCommon;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using YCIOT.ModbusPoll.RtuOverTcp;

namespace ModbusHelp
{
     public partial class SingleTest : Window
     {
          public SingleTest()
          {
               InitializeComponent();

            Core.ConnectServer();
          }

          private void start_Click(object sender, RoutedEventArgs e)
          {
            //Get_XAAS_WTPM_TrunkPressure
            //Get_XAAS_WII_WaterInjectingInstrument
            string commandType = txtCommandType.Text;
            string linkId = txtLinkId.Text;
            string modbusAddress = txtModbusAddress.Text;
            string slotId = txtSlotId.Text;

            Core.DoWork(commandType, int.Parse(linkId), int.Parse(slotId), int.Parse(modbusAddress));

            txtStatus.Text = "on working...";
          }


        public string BytesToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
               foreach (var b in bytes)
               {
                    sb.AppendFormat("{0:X2} ", b);
               }
               return sb.ToString().Trim();
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

        private void startServer_Click(object sender, RoutedEventArgs e)
        {
            Core.ConnectServer();
        }
    }
}
