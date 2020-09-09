using Acme.Common.Utils;
using HslCommunication.ModBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ModbusPoll.Vendor.XAGY
{
     public static class Box
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          public static async Task Get_XAGY_WTPM_TrunkPressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterStation = new IotDataWaterStation();
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

               var modbusAddress = par.ModbusAddress;
               ClientInfo.CurrentModbusPoolAddress = modbusAddress;

               var flag = true;
               try
               {
                    var read = await client.ReadAsync($"s={modbusAddress};x=4;10", 1);
                    if (read.IsSuccess)
                    {
                         waterStation.StationId = par.DeviceId; //注水间ID   
                         var value = client.ByteTransform.TransInt16(read.Content, 0);
                         waterStation.TrunkPressure = value / 100.0;//总汇管压力
                         waterStation.DateTime = DateTime.Now; //采集时间
                    }
                    else
                    {
                         flag = false;
                         waterStation.AlarmCode = -1;
                         waterStation.AlarmMsg = "数据异常";

                         waterStation.Mock = par.UseMockData;
                         logIotModbusPoll.DateTime = DateTime.Now;
                         logIotModbusPoll.State = -1;
                         logIotModbusPoll.Result = "读取注水间干压数据异常！";

                         redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }

                    if (flag == true || par.UseMockData)
                    {
                         waterStation.Mock = par.UseMockData;
                         waterStation.NetworkNode = ClientInfo.ManyIpAddress;
                         //用于将读取的结果写入Redis队列
                         redisClient.AddItemToList("YCIOT:IOT_Data_WaterStation", waterStation.ToJson().IndentJson());
                         redisClient.Set($"Group:WaterStation:{par.DeviceName}-{par.DeviceId}", waterStation);
                    }

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, waterStation.ToJson().IndentJson());
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

          public static async Task Get_XAGY_WII_WaterInjectingInstrument(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterWell = new IotDataWaterWell
               {
                    AlarmCode = 0,
                    AlarmMsg = ""
               };
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

               var modbusAddress = par.ModbusAddress;
               ClientInfo.CurrentModbusPoolAddress = modbusAddress;

               try
               {
                    if (!par.CommandParameter.IsNullOrEmpty())
                    {
                         var dic = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                         if (dic != null)
                         {
                              var slotId = Convert.ToInt32(dic["1"]);

                              var flag = true;

                              var startAddress = (ushort)(15 + 63 + 10 * (slotId - 1));
                              var read = await client.ReadAsync($"s={modbusAddress};x=3;{startAddress}", 10);
                              if (read.IsSuccess)
                              {
                                   waterWell.WellId = par.DeviceId; //水井ID  
                                   var value = client.ByteTransform.TransInt16(read.Content, 2);
                                   waterWell.SettedFlow = value / 100.0; //设定流量回读
                                   value = client.ByteTransform.TransInt16(read.Content, 14);
                                   waterWell.TubePressure = value / 100.0;//管压
                                   value = client.ByteTransform.TransInt16(read.Content, 4);
                                   waterWell.InstantaneousFlow = value / 100.0; //瞬时流量
                                   var value8 = client.ByteTransform.TransInt16(read.Content, 8);
                                   var value6 = client.ByteTransform.TransInt16(read.Content, 6);
                                   waterWell.CumulativeFlow = (value8 << 16 + value6) / 100.0;//表头累计
                                   value = client.ByteTransform.TransInt16(read.Content, 0);
                                   waterWell.ValveStatus = (value >> 4) & 0x01; //阀门状态
                                   waterWell.ValveMode = (value >> 1) & 0x01; //阀门工作模式
                                   waterWell.DateTime = DateTime.Now; //采集时间
                              }
                              else
                              {
                                   flag = false;
                                   waterWell.AlarmCode = -1;
                                   waterWell.AlarmMsg = "数据异常";

                                   waterWell.Mock = par.UseMockData;
                                   logIotModbusPoll.DateTime = DateTime.Now;
                                   logIotModbusPoll.State = -1;
                                   logIotModbusPoll.Result = "读取注水井数据异常！";

                                   redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());

                              }

                              waterWell.NetworkNode = ClientInfo.ManyIpAddress;

                              if (flag == true || par.UseMockData)
                              {
                                   waterWell.Mock = par.UseMockData;
                                   waterWell.NetworkNode = ClientInfo.ManyIpAddress;
                                   redisClient.AddItemToList("YCIOT:IOT_Data_WaterWell", waterWell.ToJson().IndentJson());
                                   redisClient.Set($"Group:WaterWell:{par.DeviceName}-{par.DeviceId}", waterWell);
                              }

                              //用于通过ServerEvent给调用着返回消息
                              if (!par.UserName.IsNullOrEmpty())
                              {
                                   ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, waterWell.ToJson().IndentJson());
                              }
                         }
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


          public static async Task Post_XAGY_WII_InjectionAllocation(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterWell = new IotDataWaterWell();
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

               var modbusAddress = par.ModbusAddress;
               ClientInfo.CurrentModbusPoolAddress = modbusAddress;
               try
               {
                    if (!par.CommandParameter.IsNullOrEmpty())
                    {
                         var parDict = par.CommandParameter.FromJson<Dictionary<long, double>>();
                         if (parDict != null)
                         {
                              foreach (var item in parDict)
                              {
                                   var slotId = item.Key;

                                   var startAddress = (ushort)(1 + slotId - 2);

                                   var value = (ushort)(item.Value * 100);
                                   var read = await client.WriteOneRegisterAsync($"s={par.ModbusAddress};{startAddress}", value);

                                   if (read.IsSuccess)
                                   {
                                        if (!par.UserName.IsNullOrEmpty())
                                        {
                                             ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                                        }
                                   }
                                   else
                                   {
                                        if (!par.UserName.IsNullOrEmpty())
                                        {
                                             ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, "error");

                                        }
                                   }
                              }
                         }
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

          public static async Task Post_XAGY_WII_TubePressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterWell = new IotDataWaterWell();
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

               var modbusAddress = par.ModbusAddress;
               ClientInfo.CurrentModbusPoolAddress = modbusAddress;
               try
               {
                    if (!par.CommandParameter.IsNullOrEmpty())
                    {
                         var parDict = par.CommandParameter.FromJson<Dictionary<long, double>>();
                         if (parDict != null)
                         {
                              foreach (var item in parDict)
                              {
                                   var slotId = item.Key;
                                   //原始地址：40013
                                   var startAddress = (ushort)(13 + slotId - 1);

                                   var value = (ushort)(item.Value * 100);
                                   var read = await client.WriteOneRegisterAsync($"s={par.ModbusAddress};{startAddress}", value);

                                   if (read.IsSuccess)
                                   {
                                        if (!par.UserName.IsNullOrEmpty())
                                        {
                                             ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                                        }
                                   }
                                   else
                                   {
                                        if (!par.UserName.IsNullOrEmpty())
                                        {
                                             ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, "error");
                                        }
                                   }
                              }
                         }
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
     }
}
