using Acme.Common.Utils;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ServiceModel.WaterWell;

namespace YCIOT.ModbusPoll.Utils
{
     public class Tools
     {
          public static bool IsIpAddress(string host)
          {
               return Regex.IsMatch(host, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
          }

          public static RedisEndpoint GetRedisEndpoint(RedisEndpoint redisEndpoint)
          {

               if (redisEndpoint == null)
                    return null;

               try
               {
                    if (!IsIpAddress(redisEndpoint.Host))
                    {
                         var ip = Dns.GetHostEntryAsync(redisEndpoint.Host).GetAwaiter().GetResult();
                         redisEndpoint.Host = ip.AddressList[0].ToString();
                    }
               }
               catch (Exception ex)
               {
                    Console.WriteLine(ex.Message);
                    return null;
               }

               return redisEndpoint;

          }

          //以16进制打印数据包
          public static string ToHexString(IEnumerable<byte> bytes, int length)
          {
               var byteStr = string.Empty;
               var i = 0;
               foreach (var item in bytes)
               {
                    i++;
                    if (length > 30)
                    {
                         if (i < 15)
                         {
                              byteStr += $@"{item:X2} ";
                         }
                         if (i == 15)
                         {
                              byteStr += "  ...  ";
                         }
                         if (i > length - 15)
                         {
                              byteStr += $@"{item:X2} ";
                         }
                    }
                    else
                    {
                         byteStr += $@"{item:X2} ";
                    }

                    if (i == length) break;
               }
               return byteStr;
          }

          public static IotDataWaterWell GetCacularResult(byte[] data)
          {
               //0.5配注
               //0.38瞬时
               //6682.76 当日累计
               //11.53 管压
               IotDataWaterWell waterWell = new IotDataWaterWell();
               //配注仪状态: 0
               var r0 = data[4];
               //设定流量回读: 0.17
               var t = BCDUtils.BCDToUshort((ushort)(data[5] << 8 | data[6]));

               var r1 = t / 100.0;

               //瞬时流量: 0.15
               t = BCDUtils.BCDToUshort((ushort)(data[7] << 8 | data[8]));
               var r2 = t / 100.0;


               //累计流量: 6682.89
               var t1 = BCDUtils.BCDToUshort((ushort)(data[9] << 8 | data[10]));
               var t2 = BCDUtils.BCDToUshort((ushort)(data[11] << 8 | data[12]));

               var r3 = t1 * 100 + t2 / 100.0;

               //扩展: 0.0
               //水井压力: 10.83
               t = BCDUtils.BCDToUshort((ushort)(data[17] << 8 | data[18]));
               var r4 = t / 100.0;

               waterWell.SettedFlow = r1; //设定流量回读
               waterWell.TubePressure = r4;//管压
               waterWell.InstantaneousFlow = r2; //瞬时流量
               waterWell.CumulativeFlow = r3;//表头累计

               Console.WriteLine("waterWell:" + waterWell.ToJson());
               waterWell.PrintDump();
               return waterWell;
          }

          public static float GetFloat(ushort P1, ushort P2)
          {
               int intSign, intSignRest, intExponent, intExponentRest;
               float faResult, faDigit;
               intSign = P1 / 32768;
               intSignRest = P1 % 32768;
               intExponent = intSignRest / 128;
               intExponentRest = intSignRest % 128;
               faDigit = (float)(intExponentRest * 65536 + P2) / 8388608;
               faResult = (float)Math.Pow(-1, intSign) * (float)Math.Pow(2, intExponent - 127) * (faDigit + 1);
               return faResult;
          }

          public static byte[] CDAB(byte[] source, int fromIndex = 0)
          {
               if (source.Length < 4) return source;

               byte b1 = source[fromIndex];
               byte b2 = source[fromIndex + 1];
               byte b3 = source[fromIndex + 2];
               byte b4 = source[fromIndex + 3];

               byte[] target = { b3, b4, b1, b2 };
               return target;
          }

          public static void WriteStartLog(bool type = true)
          {
               try
               {
                    var text = "";
                    if (type)
                    {
                         text = $"1 程序启动:[{DateTime.Now}]\n";
                    }
                    else
                    {
                         text = $"0 程序自动重新启动:[{DateTime.Now}]\n";
                    }
                    var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    var path = Path.Combine(executablePathRoot, "startlog.txt");
                    File.AppendAllText(path, text);
               }
               catch (Exception ex)
               {
                    $"程序启动写入日志失败:{ex.Message}".Info();
               }
          }

     }
}
