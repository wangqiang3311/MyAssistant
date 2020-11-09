using NLog;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ModbusPoll.RtuOverTcp.Utils
{
     public static class ConsoleExtension
     {
          private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
          public static void Info(this string content)
          {
               if (isDebug)
               {
                    Logger.Info(content);
               }
          }
          public static void Warn(this string content)
          {
               if (isDebug)
               {
                    Logger.Warn(content);
               }
          }
          public static void Error(this string content)
          {
               Logger.Error(content);
          }
     }
}
