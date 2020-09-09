using ModbusCommon;
using System;
using System.Configuration;
using System.Threading;

namespace ModbusConsole
{
     public class  BeatHeart 
     {
          static void Test(byte deviceId)
          {
               //从配置文件中读取ip、port、interval

               var ip = ConfigurationManager.AppSettings["ip"];
               var port = ConfigurationManager.AppSettings["port"];
               var interval= ConfigurationManager.AppSettings["interval"];

               Dowork(ip, port, interval, deviceId);

               Console.Read();
          }

          private static void Dowork(string ip, string port, string interval, byte deviceId)
          {
               string connectionErr = "";
               Sender sender = new Sender();

               var isConnected = sender.Connect(ip, int.Parse(port), out connectionErr);

               if (isConnected)
               {
                    //发送注册包
                    Regist(sender, deviceId);

                    //每隔一段时间发送注册包
                    SendBeatHeart(sender, interval,deviceId);
               }
          }

          private static void Regist(Sender sender, byte deviceId)
          {
               byte[] bytes = { 0x01, 0xC7, deviceId };
               bool isSended = sender.SendMessage(bytes);
          }

          private static void SendBeatHeart(Sender sender, string interval, byte deviceId)
          {
               while (true)
               {
                    try
                    {
                         byte[] bytes = { 0x01, 0xC8, deviceId };
                         bool isSended = sender.SendMessage(bytes);

                         Thread.Sleep(int.Parse(interval));
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"发送心跳包失败:" + ex.Message);
                    }
               }
          }
     }
}