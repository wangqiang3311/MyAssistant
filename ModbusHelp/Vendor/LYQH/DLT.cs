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
/// 电流图
/// </summary>
namespace YCIOT.ModbusPoll.Vendor.LYQH
{
     public static class DLT
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          public static async Task Get_LYQH_DLT_CurrentDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               try
               {
                    var currentDiagram = new IotDataOilWellCurrentDiagram()
                    {
                         AlarmCode = 0,
                         AlarmMsg = "正常"
                    };

                    var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                    logIotModbusPoll.State = 0;
                    logIotModbusPoll.Result = "ok";

                    var modbusAddress = par.ModbusAddress;

                    var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                    currentDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                    var slotId = Convert.ToInt32(jo1["1"].ToString());

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    currentDiagram.C = new List<double>(); //电流

                    currentDiagram.DateTime = DateTime.Now;
                    currentDiagram.WellId = par.DeviceId;

                    currentDiagram.DeviceTypeId = par.DeviceTypeId;
                    currentDiagram.Mock = false;

                    var flag = true;

                    ushort step = 100;

                    currentDiagram.Count = 200;

                    if (currentDiagram.Count <= 200 && currentDiagram.Count >= step)
                    {
                         var regAddress = (ushort)((slotId * 0x3000) + 856);//读取电流数据
                         for (ushort i = 0; i < currentDiagram.Count && flag; i += step)
                         {
                              var itemCount = (i + step > currentDiagram.Count) ? (ushort)(currentDiagram.Count - i) : step;

                              Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                              var isSuccuss = await GetCData(client, modbusAddress, currentDiagram, (ushort)(regAddress + i), itemCount,3);

                              if (!isSuccuss)
                              {
                                   var message = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个电流图电流数据异常!";
                                   FailLog.Write(redisClient, par, currentDiagram, logIotModbusPoll, "Get_LYQH_DLT_CurrentDiagram", message);
                                   flag = false;
                                   break;
                              }

                              Thread.Sleep(100);
                         }

                         if (currentDiagram.C.Count > 0)
                         {
                              currentDiagram.MaxCurrent = currentDiagram.C.Max();//最大电流
                              currentDiagram.MinCurrent = currentDiagram.C.Min();//最小电流
                              currentDiagram.AvgCurrent = Math.Round(currentDiagram.C.Average(), 2);//平均电流
                         }
                    }
                    else
                    {
                         flag = false;
                    }

                    currentDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                    if (flag == true || par.UseMockData)
                    {
                         currentDiagram.Mock = par.UseMockData;

                         if (currentDiagram.Count != null)
                              currentDiagram.D = DisplacementUtils.FitDisplacement((ushort)currentDiagram.Count, currentDiagram.Displacement);

                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_CurrentDiagram", currentDiagram.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:CurrentDiagram", currentDiagram);
                         redisClient.Set($"Single:OilWell:CurrentDiagram:{par.DeviceName}-{par.DeviceId}", currentDiagram);
                    }


                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, currentDiagram.ToJson().IndentJson());
                    }
               }
               catch (Exception ex)
               {
                    Logger.Error(ex.Message);
                    Logger.Error(ex.StackTrace);
                    Logger.Error(ex.Source);

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                    }
               }
          }
          public static async Task<bool> GetCData(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellCurrentDiagram currentDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                              //变比0.001，没有按协议文档，按实际的情况定
                              var L = value * 0.001;
                              currentDiagram.C.Add(Math.Round(L, 2));
                         }
                         else
                         {
                              currentDiagram.C.Add(value);

                         }
                    }
                    return true;
               }
               else
               {
                    tryReadTimes--;
                    if (tryReadTimes > 0)
                    {
                         return await GetCData(client, modbusAddress, currentDiagram, startAddress, regCount, tryReadTimes);
                    }
                    return false;
               }
          }

     }
}
