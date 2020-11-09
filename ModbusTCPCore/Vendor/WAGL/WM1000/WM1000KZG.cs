using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Net;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ServiceModel;
using Acme.Common.Utils;
using YCIOT.ServiceModel.OilWell;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
     // ReSharper disable once InconsistentNaming
     public static class WM1000KZG
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
          public static async Task Post_XAGL_WM1000KZG_StartWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               //用于通过ServerEvent给调用着返回消息
               if (!par.UserName.IsNullOrEmpty())
               {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
               }

               return;

               try
               {
                    var modbusAddress = par.ModbusAddress;

                    //var reg = new AMWD.Modbus.Common.Structures.Register
                    //{
                    //    Address = 4126,
                    //    HiByte = 0,
                    //    LoByte = 1
                    //};

                    //（2）写入1值到4121(Holding register)寄存器，设置油井的状态为开启。
                    // var result2 = await client.WriteAsync("4126", 1);

                    var startAddress = (ushort)4126;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    var result2 = await client.WriteOneRegisterAsync(address, (ushort)(1));


                    if (result2.IsSuccess == true)
                    {
                         //用于通过ServerEvent给调用着返回消息
                         if (!par.UserName.IsNullOrEmpty())
                         {
                              ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                         }
                    }
                    else
                    {
                         //用于通过ServerEvent给调用着返回消息
                         if (!par.UserName.IsNullOrEmpty())
                         {
                              ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, "error");
                         }
                    }

               }
               catch (Exception ex)
               {
                    Logger.Error(ex.Message);
                    Logger.Error(ex.StackTrace);
                    Logger.Error(ex.Source);

                    if (par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                    }
               }


          }

          public static async Task Post_XAGL_WM1000KZG_StopWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               //用于通过ServerEvent给调用着返回消息
               if (!par.UserName.IsNullOrEmpty())
               {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
               }

               return;

               try
               {
                    var modbusAddress = par.ModbusAddress;

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    //写入0值到4121(Holding register)寄存器，设置油井的状态为开启。
                   // var result2 = await client.WriteAsync("4126", 2);

                    var startAddress = (ushort)4126;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    var result2 = await client.WriteOneRegisterAsync(address, (ushort)(2));

                    if (result2.IsSuccess)
                    {
                         //用于通过ServerEvent给调用着返回消息
                         if (!par.UserName.IsNullOrEmpty())
                         {
                              ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                         }
                    }
                    else
                    {
                         //用于通过ServerEvent给调用着返回消息
                         if (!par.UserName.IsNullOrEmpty())
                         {
                              ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, "error");
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

               // 
          }


          public static async Task Get_XAGL_WM1000KZG_ControllerStatus(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               try
               {
                    var iotDataOilWellControllerState = new IotDataOilWellControllerState()
                    {
                         NetworkNode = ClientInfo.ManyIpAddress,
                         AlarmCode = 0,
                         AlarmMsg = "正常"
                    };

                    var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                    logIotModbusPoll.State = 0;
                    logIotModbusPoll.Result = "ok";

                    var modbusAddress = par.ModbusAddress;
                    iotDataOilWellControllerState.DeviceTypeId = par.DeviceTypeId;
                    iotDataOilWellControllerState.Mock = false;

                    var flag = true;

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    iotDataOilWellControllerState.DateTime = DateTime.Now;
                    iotDataOilWellControllerState.WellId = par.DeviceId;

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 6;
                }
                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4116", 3);

                    if (read.IsSuccess)
                    {
                         var value = client.ByteTransform.TransInt16(read.Content, 4);
                         iotDataOilWellControllerState.AlarmCode = value;

                         value = client.ByteTransform.TransInt16(read.Content, 2);
                         iotDataOilWellControllerState.StartAndStopStatus = value;

                         value = client.ByteTransform.TransInt16(read.Content, 0);
                         iotDataOilWellControllerState.LocalAndFarSwitch = value;
                    }
                    else
                    {
                         flag = false;

                         iotDataOilWellControllerState.AlarmCode = -1;
                         iotDataOilWellControllerState.AlarmMsg = "数据异常";

                         iotDataOilWellControllerState.Mock = par.UseMockData;
                         logIotModbusPoll.DateTime = DateTime.Now;
                         logIotModbusPoll.State = -1;
                         logIotModbusPoll.Result = "读取变频器状态数据异常！";

                         redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }

                    iotDataOilWellControllerState.NetworkNode = ClientInfo.ManyIpAddress;

                    //Redis中写入设备最后状态

                    if (flag == true || par.UseMockData)
                    {
                         iotDataOilWellControllerState.Mock = par.UseMockData;
                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_ControllerState", iotDataOilWellControllerState.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:ControllerState", iotDataOilWellControllerState);
                         redisClient.Set($"Single:OilWell:ControllerState:{par.DeviceName}-{par.DeviceId}", iotDataOilWellControllerState);
                    }

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, iotDataOilWellControllerState.ToJson().IndentJson());
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
