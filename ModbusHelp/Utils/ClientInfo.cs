using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace YCIOT.ModbusPoll.RtuOverTcp.Utils
{
     public enum ServerState
     {
          NotStarted = 0,
          Started = 1,
          Starting = 2,
     }

     public static class ClientInfo
     {
          public static int CurrentModbusPoolAddress;
          public static int? LinkId;
          /// <summary>
          /// 对外只暴露IP
          /// </summary>
          public static string ManyIpAddress
          {
               get
               {
                    IPEndPoint result;
                    IPEndPoint.TryParse(IpAddress, out result);
                    return result?.Address.ToString();
               }
          }

          public static string IpAddress;


          public static DateTime RequestTime;

          public static int ExpectedType;

          public static int ExpectedDataLen;

          public static System.Runtime.Caching.MemoryCache cache;

          public static object locker = new object();

          public static bool HasValue(bool isUselink)
          {
               if (isUselink)
               {
                    if (LinkId == null || IpAddress == null)
                    {
                         return false;
                    }
                    return true;


               }
               else
               {
                    if (IpAddress == null || RequestTime == null)
                    {
                         return false;
                    }
                    return true;
               }
          }

          public static void Clear()
          {
               LinkId = null;
               IpAddress = null;
          }
     }
}
