using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Acme.Common.Utils;
using HslCommunication.ModBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

/// <summary>
/// 油井压力
/// </summary>
namespace YCIOT.ModbusPoll.Vendor.LYQH
{
     public static class YJYL
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          public static async Task Get_LYQH_GJL_Pressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               try
               {
                    IotDataOilWellPressure oillPress = new IotDataOilWellPressure
                    {
                         AlarmCode = 0,
                         AlarmMsg = "正常"
                    };

                    var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                    var modbusAddress = par.ModbusAddress;

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    oillPress.WellId = par.DeviceId;

                    oillPress.DeviceTypeId = par.DeviceTypeId;
                    oillPress.Mock = false;

                    var failed = await SetPressure(redisClient, client, modbusAddress, oillPress, logIotModbusPoll, par);

                    oillPress.NetworkNode = ClientInfo.ManyIpAddress;

                    //用于将读取的结果写入Redis队列 
                    if (!failed || par.UseMockData)
                    {
                         oillPress.Mock = par.UseMockData;
                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_Pressure", oillPress.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:Pressure", oillPress);
                         redisClient.Set($"Single:OilWell:Pressure:{par.DeviceName}-{par.DeviceId}", oillPress);
                    }

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, failed ? -2 : 0, oillPress.ToJson().IndentJson());
                    }
               }
               catch (Exception ex)
               {
                    Logger.Error(ex.Message);
                    Logger.Error(ex.StackTrace);
                    Logger.Error(ex.Source);

                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                    }
               }
          }


          public static async Task<bool> GetOilPress(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPressure pressure, ushort startAddress, ushort regCount, int tryReadTimes)
          {
            lock (ClientInfo.locker)
            {
                ClientInfo.RequestTime = DateTime.Now;
                ClientInfo.ExpectedType = 0x04;
                ClientInfo.ExpectedDataLen = regCount * 2;
            }
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);
               if (read.IsSuccess)
               {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);

                    pressure.Pressure = value;
                    return true;
               }
               else
               {
                    tryReadTimes--;
                    if (tryReadTimes > 0)
                    {
                         return await GetOilPress(client, modbusAddress, pressure, startAddress, regCount, tryReadTimes);
                    }
                    return false;
               }
          }

          public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPressure pressure, ushort startAddress, ushort regCount, int tryReadTimes)
          {
            lock (ClientInfo.locker)
            {
                ClientInfo.RequestTime = DateTime.Now;
                ClientInfo.ExpectedType = 0x04;
                ClientInfo.ExpectedDataLen = regCount * 2;
            }
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);
               if (read.IsSuccess)
               {
                    var year = client.ByteTransform.TransByte(read.Content, 0);
                    var month = client.ByteTransform.TransByte(read.Content, 1);
                    var date = client.ByteTransform.TransByte(read.Content, 2);
                    var h = client.ByteTransform.TransByte(read.Content, 3);
                    var m = client.ByteTransform.TransByte(read.Content, 4);
                    var s = client.ByteTransform.TransByte(read.Content, 5);

                    pressure.DateTime = new DateTime(2000 + year, month, date, h, m, s);

                    return true;
               }
               else
               {
                    tryReadTimes--;
                    if (tryReadTimes > 0)
                    {
                         return await GetDateTime(client, modbusAddress, pressure, startAddress, regCount, tryReadTimes);
                    }
                    return false;
               }
          }

          public static async Task<bool> SetPressure(RedisClient redisClient, ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPressure pressure, LogIotModbusPoll logIotModbusPoll, ControlRequest par, int tryReadTimes = 3)
          {
               //获取采集时间

               var failed = await GetDateTime(client, modbusAddress, pressure, (ushort)1, 3, tryReadTimes);

               if (failed)
               {
                    FailLog.Write(redisClient, par, pressure, logIotModbusPoll, "Get_LYQH_GJL_Pressure", "读取油井压力数据异常！");
                    return false;
               }

               failed = await GetOilPress(client, modbusAddress, pressure, (ushort)0, 1, tryReadTimes);

               if (failed)
               {
                    FailLog.Write(redisClient, par, pressure, logIotModbusPoll, "Get_LYQH_GJL_Pressure", "读取油井压力数据异常！");
                    return false;
               }
               return true;
          }

     }
}
