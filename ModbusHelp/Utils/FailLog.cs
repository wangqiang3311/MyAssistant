using Acme.Common.Utils;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Text;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ModbusPoll.Utils
{
     public class FailLog
     {
          public static void Write(RedisClient redisClient, ControlRequest par, dynamic table, LogIotModbusPoll logIotModbusPoll, string method, string message)
          {
               string errorMsg = table.AlarmMsg;
               string msg = $"{par.DeviceName}-{par.DeviceId}-{par.ModbusAddress}";

               table.AlarmCode = -1;
               table.AlarmMsg = "数据异常";

               if (errorMsg == "正常") errorMsg = table.AlarmMsg;
               table.Mock = par.UseMockData;
              
               logIotModbusPoll.Type = method;
               logIotModbusPoll.DateTime = DateTime.Now;
               logIotModbusPoll.State = -1;
               logIotModbusPoll.Result = $"{msg}:{message}[{errorMsg}]";

               logIotModbusPoll.Result.Info();

               redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
          }
     }
}
