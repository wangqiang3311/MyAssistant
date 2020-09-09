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

namespace ModbusHelp
{
     public partial class MainWindow : Window
     {
          public MainWindow()
          {
               InitializeComponent();
          }

          private void start_Click(object sender, RoutedEventArgs e)
          {
               string ip = txtIP.Text;
               string port = txtPort.Text;
               string clientCount = txtClientCount.Text;
               string interval = txtInterval.Text;


               for (int i = 0; i < int.Parse(clientCount); i++)
               {
                    Task.Factory.StartNew(() =>
                    {
                         var j = i;
                         Dowork(ip, port, interval, j);
                    });
               }

               txtStatus.Text = "on working...";
          }

          private void Dowork(string ip, string port, string interval, int index)
          {
               string connectionErr = "";
               Sender ipadSender = new Sender();

               var isConnected = ipadSender.Connect(ip, int.Parse(port), out connectionErr);

               if (isConnected)
               {
                    //发数据
                    SendMessage(ipadSender);
                    ReceiveAndResponse(ipadSender);
               }
          }
          /// <summary>
          /// 模拟设备接收命令并返回数据
          /// </summary>
          /// <param name="ipadSender"></param>
          public void ReceiveAndResponse(Sender ipadSender)
          {
               Task.Run(function: async () =>
               {
                    while (true)
                    {
                         try
                         {
                              var bytes = await ipadSender.ReceiveMessage();

                              if (bytes == null) continue;
                              if (bytes.Length != 8) continue;

                              //33 01 08 0A 00 01 DB BA
                              if (BytesToHexString(bytes).EndsWith("DB BA"))
                              {
                                   var data = HexStringToBytes("33 01 01 0B 1E F7");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }
                              //34 01 08 0A 00 01 DA 0D
                              if (BytesToHexString(bytes).EndsWith("DA 0D"))
                              {
                                   var data = HexStringToBytes("34 01 01 0B 1F 83");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //33 01 0A 08 00 08 BB C4
                              if (BytesToHexString(bytes).EndsWith("BB C4"))
                              {
                                   var data = HexStringToBytes("33 01 01 B0 5E 84");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //34 01 0A 08 00 08 BA 73
                              if (BytesToHexString(bytes).EndsWith("BA 73"))
                              {
                                   var data = HexStringToBytes("34 01 01 B0 5F F0");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }



                              //33 01 05 00 00 01 F9 14
                              if (BytesToHexString(bytes).EndsWith("F9 14"))
                              {
                                   var data = HexStringToBytes("33 01 01 00 5F 30");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //34 01 05 00 00 01 F8 A3
                              if (BytesToHexString(bytes).EndsWith("F8 A3"))
                              {
                                   var data = HexStringToBytes("34 01 01 00 5E 44");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }


                              //33 01 05 01 00 01 A8 D4
                              if (BytesToHexString(bytes).EndsWith("A8 D4"))
                              {
                                   var data = HexStringToBytes("33 01 01 00 5F 30");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }


                              //34 01 05 01 00 01 A9 63
                              if (BytesToHexString(bytes).EndsWith("A9 63"))
                              {
                                   var data = HexStringToBytes("34 01 01 00 5E 44");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //33 03 10 00 00 1A C4 D3
                              if (BytesToHexString(bytes).EndsWith("C4 D3"))
                              {
                                   var data = HexStringToBytes("33 03 34 31 90 3F 17 22 00 3E FD 66 66 41 E2 00 00 42 3C 00 00 00 00 45 49 3F 0B A8 C1 41 1C 6D 5D 41 87 4B C7 41 6F EE CC 40 81 00 00 00 00 99 9A 41 CD 99 9A 41 61 DC 01");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //34 03 10 00 00 1A C5 64
                              if (BytesToHexString(bytes).EndsWith("C5 64"))
                              {
                                   var data = HexStringToBytes("34 03 34 31 90 3F 17 22 00 3E FD 66 66 41 E2 00 00 42 3C 00 00 00 00 45 49 3F 0B A8 C1 41 1C 6D 5D 40 88 4B C7 41 6F EE CC 40 81 00 00 00 00 99 9A 41 CD 99 9A 41 61 A7 D0");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }



                              //33 03 11 98 00 32 44 DE
                              if (BytesToHexString(bytes).EndsWith("44 DE"))
                              {
                                   var data = HexStringToBytes("33 03 64 56 79 42 0D FD F4 3C 54 C6 A7 3E 8B 76 5A 41 49 BB 11 41 6A 99 9A 3F D9 64 61 3F AB 00 00 3F C0 00 00 00 01 00 00 41 20 66 66 41 86 00 11 00 11 00 02 00 02 00 04 00 07 00 00 00 07 00 00 00 0A 00 00 00 00 00 00 00 00 00 01 00 00 00 00 41 70 00 00 41 A0 80 00 44 BB 80 00 45 3B 99 9A 3E 19 33 33 3F 33 99 87");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }


                              //34 03 11 98 00 32 44 DE
                              if (BytesToHexString(bytes).EndsWith("45 69"))
                              {
                                   var data = HexStringToBytes("34 03 64 56 79 42 0D FD F4 3C 54 C6 A7 3E 8B 76 5A 41 49 BB 11 41 6A 99 9A 3F D9 64 61 3F AB 00 00 3F C0 00 00 00 01 00 00 41 21 66 67 41 88 00 11 00 11 00 02 00 02 00 04 00 07 00 00 00 07 00 00 00 0A 00 00 00 00 00 00 00 00 00 01 00 00 00 00 41 70 00 00 41 A0 80 00 44 BB 80 00 45 3B 99 9A 3E 19 33 33 3F 33 01 D5");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }


                              //33 03 11 D6 00 04 A4 DF
                              if (BytesToHexString(bytes).EndsWith("A4 DF"))
                              {
                                   var data = HexStringToBytes("33 03 08 00 00 42 A0 00 00 42 48 DE C2");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }

                              //34 03 11 D6 00 04 A5 68
                              if (BytesToHexString(bytes).EndsWith("A5 68"))
                              {
                                   var data = HexStringToBytes("34 03 08 00 00 42 A1 00 00 42 49 38 B6");

                                   var isSended = ipadSender.SendMessage(data);

                                   if (isSended)
                                   {
                                        Console.WriteLine("发送成功");
                                   }
                              }
                         }
                         catch (Exception ex)
                         {
                              Console.WriteLine();

                              this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                              {
                                   txtStatus.Text = $"客户端发送数据出错:" + ex.Message;
                              });

                         }
                    }

               });
          }


          /// <summary>
          /// 模拟One发数据
          /// </summary>
          /// <param name="ipadSender"></param>
          public void SendMessage(Sender ipadSender)
          {
               Task.Run(() =>
               {
                    while (true)
                    {
                         try
                         {
                              byte[] bytes = { 01, 04, 0x14, 00, 00, 00, 0x62, 00, 0x62, 00, 0x95, 0x45, 0x24, 00, 00, 00, 00, 07, 0x60, 00, 00, 00, 00, 01, 0xFE };
                              bool isSended = ipadSender.SendMessage(bytes);

                              Thread.Sleep(200);
                         }
                         catch (Exception ex)
                         {
                              Console.WriteLine();

                              this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                              {
                                   txtStatus.Text = $"客户端发送数据出错:" + ex.Message;
                              });

                         }
                    }

               });
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
     }
}
