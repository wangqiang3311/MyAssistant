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
using YCIOT.ServiceModel.IOT;

//功率图
namespace YCIOT.ModbusPoll.Vendor.LYQH
{
     public static class GLT
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          public static async Task Get_LYQH_GLT_PowerDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               try
               {
                    var powerDiagram = new IotDataOilWellPowerDiagram()
                    {
                         AlarmCode = 0,
                         AlarmMsg = "正常"
                    };

                    var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                    logIotModbusPoll.State = 0;
                    logIotModbusPoll.Result = "ok";

                    var modbusAddress = par.ModbusAddress;
                    powerDiagram.DeviceTypeId = par.DeviceTypeId;
                    powerDiagram.Mock = false;

                    var flag = true;

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                    powerDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());
                    var slotId = Convert.ToInt32(jo1["1"].ToString());

                    powerDiagram.D = new List<double>();  //位移
                    powerDiagram.P = new List<double>(); //功率

                    powerDiagram.DateTime = DateTime.Now;
                    powerDiagram.WellId = par.DeviceId;


                    var regAddress = (ushort)((slotId * 0x3000) + 256);  //读取功率数据
                    var isSuccess = await GetDateTime(client, modbusAddress, powerDiagram, regAddress, 3, 3);

                    if (!isSuccess)
                    {
                         FailLog.Write(redisClient, par, powerDiagram, logIotModbusPoll, "Get_LYQH_GLT_PowerDiagram", "读取功率图时间异常");
                    }

                    ushort step = 100;
                    powerDiagram.Count = 200;
                    if (powerDiagram.Count <= 300 && powerDiagram.Count >= step)
                    {
                         regAddress = (ushort)((slotId * 0x3000) + 1356);  //读取功率数据
                         for (ushort i = 0; i < powerDiagram.Count && flag; i += step)
                         {
                              var itemCount = (i + step > powerDiagram.Count) ? (ushort)(powerDiagram.Count - i) : step;

                              Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                              isSuccess = await GetPData(client, modbusAddress, powerDiagram, (ushort)(regAddress + i), itemCount, 3);

                              if (!isSuccess)
                              {
                                   var message = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个功率图功率数据异常!";
                                   FailLog.Write(redisClient, par, powerDiagram, logIotModbusPoll, "Get_LYQH_GLT_PowerDiagram", message);
                                   flag = false;
                                   break;
                              }

                              Thread.Sleep(100);
                         }

                         if (powerDiagram.P.Count > 0)
                         {
                              powerDiagram.MaxPower = powerDiagram.P.Max(); //最大功率
                              powerDiagram.MinPower = powerDiagram.P.Min(); //最小功率
                              powerDiagram.AvgPower = Math.Round(powerDiagram.P.Average(), 2); //平均功率
                         }
                    }
                    else
                    {
                         flag = false;
                    }

                    powerDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                    //用于将读取的结果写入Redis队列

                    if (flag == true || par.UseMockData)
                    {
                         powerDiagram.Mock = par.UseMockData;

                         if (powerDiagram.Count != null)
                              powerDiagram.D =
                                  DisplacementUtils.FitDisplacement((ushort)powerDiagram.Count, (double)powerDiagram.Displacement);
                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_PowerDiagram", powerDiagram.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:PowerDiagram", powerDiagram);
                         redisClient.Set($"Single:OilWell:PowerDiagram:{par.DeviceName}-{par.DeviceId}", powerDiagram);
                    }


                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, powerDiagram.ToJson().IndentJson());
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

          public static async Task<bool> GetPData(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPowerDiagram powerDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                    for (var j = 0; j < regCount; j++)
                    {
                         var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                         if (value != 0)
                         {
                              var L = value * 0.01;
                              powerDiagram.P.Add(Math.Round(L, 2));
                         }
                         else
                         {
                              powerDiagram.P.Add(value);
                         }
                    }
                    return true;
               }
               else
               {
                    tryReadTimes--;
                    if (tryReadTimes > 0)
                    {
                         return await GetPData(client, modbusAddress, powerDiagram, startAddress, regCount, tryReadTimes);
                    }
                    return false;
               }
          }

          public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPowerDiagram powerDiagram, ushort startAddress, ushort regCount, int tryReadTimes = 3)
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

                    powerDiagram.DateTime = new DateTime(2000 + year, month, date, h, m, s);
                    return true;
               }
               else
               {
                    tryReadTimes--;
                    if (tryReadTimes > 0)
                    {
                         return await GetDateTime(client, modbusAddress, powerDiagram, startAddress, regCount, tryReadTimes);
                    }
                    return false;
               }
          }


     }
}
