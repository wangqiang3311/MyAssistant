using Acme.Common;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ServiceModel.IOT;
using BCDUtils = Acme.Common.Utils.BCDUtils;

//延安华圣
namespace YCIOT.ModbusPoll.Vendor.YAHS
{
     public static class Box
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          //获取汇管压力
          public static async Task Get_YAHS_WTPM_TrunkPressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterStation = new IotDataWaterStation();
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();
               var modbusAddress = par.ModbusAddress;

               ClientInfo.LinkId = par.LinkId;
               ClientInfo.CurrentModbusPoolAddress = modbusAddress;

               var flag = true;
               try
               {
                    var startAddress = (ushort)10;
                    lock (ClientInfo.locker)
                    {
                         ClientInfo.RequestTime = DateTime.Now;
                         ClientInfo.ExpectedType = 0x04;
                         ClientInfo.ExpectedDataLen = 0x08;
                    }
                    var read = await client.ReadAsync($"s={par.ModbusAddress};x=4;{startAddress}", 4);
                    if (read.IsSuccess)
                    {
                         waterStation.StationId = par.DeviceId; //注水间ID  
                         var value1 = client.ByteTransform.TransInt16(read.Content, 0);
                         var value2 = client.ByteTransform.TransInt16(read.Content, 2);
                         var value3 = client.ByteTransform.TransInt16(read.Content, 4);
                         var value4 = client.ByteTransform.TransInt16(read.Content, 6);

                         var v1 = BCDUtils.BCDToUshort((ushort)value1) / 100.0;
                         var v2 = BCDUtils.BCDToUshort((ushort)value2) / 100.0;
                         var v3 = BCDUtils.BCDToUshort((ushort)value3) / 100.0;
                         var v4 = BCDUtils.BCDToUshort((ushort)value4) / 100.0;

                         double[] values = { v1, v2, v3, v4 };

                         waterStation.TrunkPressure = values.Max();//总汇管压力
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

                         string msg = $"{par.DeviceName}-{par.DeviceId}";
                         Logger.Warn($"{msg}:读取注水间干压数据异常！[{read.Message}]");

                         logIotModbusPoll.Result = $"{msg}:读取注水间干压数据异常！[{read.Message}]";

                         redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }

                    waterStation.NetworkNode = ClientInfo.ManyIpAddress;
                    waterStation.StationId = par.DeviceId;

                    if (flag == true || par.UseMockData)
                    {
                         waterStation.Mock = par.UseMockData;
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

          //获取注水仪数据
          public static async Task Get_YAHS_WII_WaterInjectingInstrument(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var waterWell = new IotDataWaterWell();
               var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();
               //par.ModbusAddress = 1;
               var modbusAddress = par.ModbusAddress;
               ClientInfo.LinkId = par.LinkId;

               ClientInfo.CurrentModbusPoolAddress = modbusAddress;

               var flag = true;

               try
               {
                    if (!par.CommandParameter.IsNullOrEmpty())
                    {
                         var parDict = par.CommandParameter.FromJson<Dictionary<long, string>>();
                         if (parDict != null)
                         {
                              var startAddress = (ushort)10;
                              lock (ClientInfo.locker)
                              {
                                   ClientInfo.RequestTime = DateTime.Now;
                                   ClientInfo.ExpectedType = 0x04;
                                   ClientInfo.ExpectedDataLen = 0x08;
                              }
                              var read = await client.ReadAsync($"s={par.ModbusAddress};x=4;{startAddress}", 4);
                              if (read.IsSuccess)
                              {
                                   var value1 = client.ByteTransform.TransInt16(read.Content, 0);
                                   var value2 = client.ByteTransform.TransInt16(read.Content, 2);
                                   var value3 = client.ByteTransform.TransInt16(read.Content, 4);
                                   var value4 = client.ByteTransform.TransInt16(read.Content, 6);

                                   var v1 = BCDUtils.BCDToUshort((ushort)value1) / 100.0;
                                   var v2 = BCDUtils.BCDToUshort((ushort)value2) / 100.0;
                                   var v3 = BCDUtils.BCDToUshort((ushort)value3) / 100.0;
                                   var v4 = BCDUtils.BCDToUshort((ushort)value4) / 100.0;

                                   double[] values = { v1, v2, v3, v4 };

                                   waterWell.TrunkPressure = values.Max();//总汇管压力
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

                                   string msg = $"{par.DeviceName}-{par.DeviceId}";
                                   Logger.Warn($"{msg}:读取注水间干压数据异常！[{read.Message}]");

                                   logIotModbusPoll.Result = $"{msg}:读取注水间干压数据异常！[{read.Message}]";

                                   redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                              }

                              var dic = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                              if (dic != null)
                              {
                                   var slotId = Convert.ToInt32(dic["1"]);
                              
                                   flag = true;
                                   startAddress = (ushort)(14 + 10 * (slotId - 1));
                                   lock (ClientInfo.locker)
                                   {
                                        ClientInfo.RequestTime = DateTime.Now;
                                        ClientInfo.ExpectedType = 0x04;
                                        ClientInfo.ExpectedDataLen = 0x14 + 2;
                                   }
                                   read = await client.ReadAsync($"s={par.ModbusAddress};x=4;{startAddress}", 11);

                                   if (read.IsSuccess)
                                   {
                                        waterWell.WellId = par.DeviceId; //水井ID   

                                        var value = client.ByteTransform.TransInt16(read.Content, 0);

                                        waterWell.ValveStatus = (BCDUtils.BCDToUshort(read.Content[0]) >> 4) & 0x01; //阀门状态
                                        waterWell.ValveMode = (BCDUtils.BCDToUshort(read.Content[0]) >> 1) & 0x01; //阀门工作模式

                                        value = client.ByteTransform.TransInt16(read.Content, 2);
                                        waterWell.SettedFlow = BCDUtils.BCDToUshort((ushort)value) / 100.0; //设定流量回读

                                        value = client.ByteTransform.TransInt16(read.Content, 4);
                                        waterWell.InstantaneousFlow = BCDUtils.BCDToUshort((ushort)value) / 100.0; //瞬时流量

                                        var value1 = client.ByteTransform.TransInt16(read.Content, 8);
                                        var value2 = client.ByteTransform.TransInt16(read.Content, 6);
                                        waterWell.CumulativeFlow = BCDUtils.BCDToUshort((ushort)value1) * 100 + BCDUtils.BCDToUshort((ushort)value2) / 100.0;//表头累计

                                        value = client.ByteTransform.TransInt16(read.Content, 14);

                                        waterWell.TubePressure = BCDUtils.BCDToUshort((ushort)value) / 100.0;//管压
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

                                        string msg = $"{par.DeviceName}-{par.DeviceId}";

                                        Logger.Warn($"{msg}:读取注水井数据异常！[{read.Message}]");

                                        logIotModbusPoll.Result = $"{msg}:读取注水井数据异常！[{read.Message}]";

                                        redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());

                                   }
                                   waterWell.NetworkNode = ClientInfo.ManyIpAddress;

                                   if (flag == true || par.UseMockData)
                                   {
                                        waterWell.Mock = par.UseMockData;
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

          //设置 （下发）配注量
          public static async Task Post_YAHS_WII_InjectionAllocation(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               var modbusAddress = par.ModbusAddress;
               par.ModbusAddress = 1;
               ClientInfo.LinkId = par.LinkId;
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
                                   var startAddress = (ushort)(0 + slotId - 1);
                                   var value = BCDUtils.UshortToBCD((ushort)(item.Value * 100));
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
                                             ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");

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

          //下发汇管压力
          public static async Task Post_YAHS_WII_TubePressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               var modbusAddress = par.ModbusAddress;
               par.ModbusAddress = 1;
               ClientInfo.LinkId = par.LinkId;
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
                                   var slotId = item.Value;
                                   var t = item.Value;
                                   var startAddress = (ushort)(12 + slotId - 1);
                                   var value = BCDUtils.UshortToBCD((ushort)(item.Value * 100));

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
