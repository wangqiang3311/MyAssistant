using Acme.Common.Utils;
using HslCommunication.ModBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ServiceModel.IOT;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
     // ReSharper disable once InconsistentNaming
     public static class WM1000YXGT
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
          private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");

          public static async Task Get_XAGL_WM1000YXGT_IndicatorDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();

               try
               {
                    var indicatorDiagram = new IotDataOilWellIndicatorDiagram()
                    {
                         AlarmCode = 0,
                         AlarmMsg = "正常"
                    };

                    var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                    logIotModbusPoll.State = 0;
                    logIotModbusPoll.Result = "ok";

                    var modbusAddress = par.ModbusAddress;
                    indicatorDiagram.DeviceTypeId = par.DeviceTypeId;
                    indicatorDiagram.Mock = false;

                    var flag = true;
                    var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                    indicatorDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    indicatorDiagram.D = new List<double>();  //位移
                    indicatorDiagram.L = new List<double>();  //载荷

                    indicatorDiagram.DateTime = DateTime.Now;

                    indicatorDiagram.WellId = par.DeviceId;
                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 2;
                }
                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4117", 1);

                    if (read.IsSuccess)
                    {
                         var value = client.ByteTransform.TransInt16(read.Content, 0);
                         if (value == 0)
                         {
                              indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;
                              indicatorDiagram.AlarmCode = 3;
                              indicatorDiagram.AlarmMsg = "停井";

                              redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram",
                                  indicatorDiagram.ToJson().IndentJson());
                              redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                              redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);

                              if (!par.UserName.IsNullOrEmpty())
                              {
                                   ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0,
                                       indicatorDiagram.ToJson().IndentJson());
                              }

                              return;
                         }
                    }
                    else
                    {
                         flag = false;

                         indicatorDiagram.AlarmCode = -1;
                         indicatorDiagram.AlarmMsg = "数据异常";

                         logIotModbusPoll.Type = "Get_XAGL_WM1000YXGT_IndicatorDiagram";
                         logIotModbusPoll.DateTime = DateTime.Now;
                         indicatorDiagram.Mock = par.UseMockData;
                         logIotModbusPoll.State = -1;
                         logIotModbusPoll.Result = $"读取采样间隔数据异常！{read.Message}";

                         redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());

                    }

                    if (flag)
                    {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 2;
                    }
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;8097", 1);

                         if (read.IsSuccess)
                         {
                              var value = client.ByteTransform.TransInt16(read.Content, 0);
                              indicatorDiagram.Count = 250;
                              indicatorDiagram.Interval = Math.Round(value * 0.01, 3);
                              indicatorDiagram.Stroke = Math.Round((60.0 / ((int)indicatorDiagram.Count * (float)indicatorDiagram.Interval)), 2);
                         }
                         else
                         {
                              flag = false;

                              indicatorDiagram.AlarmCode = -1;
                              indicatorDiagram.AlarmMsg = "数据异常";

                              indicatorDiagram.Mock = par.UseMockData;
                              logIotModbusPoll.Type = "Get_XAGL_WM1000YXGT_IndicatorDiagram";
                              logIotModbusPoll.DateTime = DateTime.Now;
                              logIotModbusPoll.State = -1;
                              logIotModbusPoll.Result = $"读取采样间隔数据异常！[{read.Message}]";

                              redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                         }
                    }

                    const ushort step = 100;
                    if (flag && indicatorDiagram.Count <= 300 && indicatorDiagram.Count >= step)
                    {
                         const ushort regAddress = 38268; //读取载荷数据

                         for (ushort i = 0; i < indicatorDiagram.Count && flag; i += step)
                         {
                              var itemCount = (i + step > indicatorDiagram.Count) ? (ushort)(indicatorDiagram.Count - i) : step;
                              if (isDebug)
                                   Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                        lock (ClientInfo.locker)
                        {
                            ClientInfo.RequestTime = DateTime.Now;
                            ClientInfo.ExpectedType = 0x03;
                            ClientInfo.ExpectedDataLen =itemCount* 2;
                        }
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;{(regAddress + i)}", itemCount);

                              if (read.IsSuccess)
                              {
                                   for (var j = 0; i < itemCount; i++)
                                   {
                                        var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                                        if (value == 0)
                                        {
                                             indicatorDiagram.L.Add(value);
                                        }
                                        else
                                        {
                                             var L = (value - 800) * 150 / 3200.0;
                                             indicatorDiagram.L.Add(Math.Round(L, 2));
                                        }
                                   }
                              }
                              else
                              {
                                   flag = false;

                                   indicatorDiagram.AlarmCode = -1;
                                   indicatorDiagram.AlarmMsg = "数据异常";

                                   indicatorDiagram.Mock = par.UseMockData;
                                   logIotModbusPoll.Type = "Get_XAGL_WM1000YXGT_IndicatorDiagram";
                                   logIotModbusPoll.DateTime = DateTime.Now;
                                   logIotModbusPoll.State = -1;
                                   logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + $" 个有线功图载荷数据异常!{read.Message}";

                                   redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                              }

                              Thread.Sleep(100);
                         }

                         //读取冲程
                         indicatorDiagram.MaxLoad = Math.Round(indicatorDiagram.L.Max(), 2);//最大载荷
                         indicatorDiagram.MinLoad = Math.Round(indicatorDiagram.L.Min(), 2);//最小载荷
                         indicatorDiagram.AvgLoad = Math.Round(indicatorDiagram.L.Average(), 2);//平均载荷

                         indicatorDiagram.D.Add(indicatorDiagram.D[0]);
                         indicatorDiagram.L.Add(indicatorDiagram.L[0]);
                    }
                    else
                    {
                         flag = false;
                    }

                    indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                    //用于将读取的结果写入Redis队列
                    if (flag == true || par.UseMockData)
                    {
                         indicatorDiagram.Mock = par.UseMockData;

                         if (indicatorDiagram.Count != null)
                              indicatorDiagram.D = DisplacementUtils.FitDisplacement((ushort)indicatorDiagram.Count, (double)indicatorDiagram.Displacement);

                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                         redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);
                    }

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, indicatorDiagram.ToJson().IndentJson());
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
