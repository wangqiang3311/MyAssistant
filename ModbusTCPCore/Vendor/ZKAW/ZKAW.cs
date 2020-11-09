using Acme.Common.Utils;
using HslCommunication.ModBus;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

//中科奥威
namespace YCIOT.ModbusPoll.Vendor.ZKAW
{
     public static class IndicatorDiagram
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

          public static async Task Get_ZKAW_IndicatorDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                    var modbusAddress = par.ModbusAddress;
                    indicatorDiagram.DeviceTypeId = par.DeviceTypeId;
                    indicatorDiagram.Mock = false;

                    ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                    indicatorDiagram.D = new List<double>();  //位移
                    indicatorDiagram.L = new List<double>();  //载荷

                    indicatorDiagram.DateTime = DateTime.Now;
                    indicatorDiagram.WellId = par.DeviceId;

                    logIotModbusPoll.State = 0;
                    logIotModbusPoll.Result = "ok";

                    var flag = true;

                    var read = await client.ReadAsync($"s={modbusAddress};x=3;980", 4);//功图实际点数
                    if (read.IsSuccess)
                    {
                         var value = client.ByteTransform.TransInt16(read.Content, 6);
                         indicatorDiagram.Count = value; //采样点数
                         value = client.ByteTransform.TransInt16(read.Content, 0);
                         indicatorDiagram.Interval = value * 0.001; //采样间隔
                    }
                    else
                    {
                         flag = false;

                         indicatorDiagram.AlarmCode = -1;
                         indicatorDiagram.AlarmMsg = "数据异常";

                         indicatorDiagram.Mock = par.UseMockData;
                         logIotModbusPoll.State = -1;
                         logIotModbusPoll.Result = "读取采样间隔、采样点数数据异常！";

                         redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }

                    read = await client.ReadAsync($"s={modbusAddress};x=3;990", 2);//冲次

                    if (read.IsSuccess)
                    {
                         var value = client.ByteTransform.TransInt16(read.Content, 0);
                         indicatorDiagram.Stroke = value;//冲次
                         value = client.ByteTransform.TransInt16(read.Content, 2);
                         indicatorDiagram.Displacement = value; //冲程
                    }
                    else
                    {
                         flag = false;

                         indicatorDiagram.AlarmCode = -1;
                         indicatorDiagram.AlarmMsg = "数据异常";

                         indicatorDiagram.Mock = par.UseMockData;
                         logIotModbusPoll.State = -1;
                         logIotModbusPoll.Result = "读取冲次、冲程数据异常！";

                         redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }

                    const ushort step = 50;
                    if (flag)
                    {
                         //读取载荷数据
                         ushort regAddress = 1250;
                         for (ushort i = 0; i < indicatorDiagram.Count; i += step)
                         {
                              var itemCount = (i + step > indicatorDiagram.Count)
                                  ? (ushort)(indicatorDiagram.Count - i)
                                  : step;

                              Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                              read = await client.ReadAsync($"s={modbusAddress};x=3;{(ushort)(regAddress + i)}", itemCount);

                              if (!read.IsSuccess)
                              {
                                   read = await client.ReadAsync($"s={modbusAddress};x=3;{(ushort)(regAddress + i)}", itemCount);
                              }

                              if (read.IsSuccess)
                              {
                                   for (var j = 0; j < itemCount; j++)
                                   {
                                        var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                                        indicatorDiagram.L.Add(Math.Round(value * 0.01, 3));
                                   }
                              }
                              else
                              {
                                   flag = false;

                                   indicatorDiagram.AlarmCode = -1;
                                   indicatorDiagram.AlarmMsg = "数据异常";

                                   indicatorDiagram.Mock = par.UseMockData;
                                   logIotModbusPoll.DateTime = DateTime.Now;
                                   logIotModbusPoll.State = -1;
                                   logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " +
                                                             itemCount.ToString() + " 个有线功图载荷数据异常!";

                                   redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll",
                                       logIotModbusPoll.ToJson().IndentJson());
                              }

                              Thread.Sleep(20);
                         }
                    }

                    if (flag)
                    {

                         //读取位移数据
                         var regAddress = 1000;
                         for (ushort i = 0; i < indicatorDiagram.Count; i += step)
                         {
                              var itemCount = (i + step > indicatorDiagram.Count)
                                  ? (ushort)(indicatorDiagram.Count - i)
                                  : step;

                              Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                              read = await client.ReadAsync($"s={modbusAddress};x=3;{(ushort)(regAddress + i)}", itemCount);

                              if (!read.IsSuccess)
                              {
                                   read = await client.ReadAsync($"s={modbusAddress};x=3;{(ushort)(regAddress + i)}", itemCount);
                              }
                              if (read.IsSuccess)
                              {
                                   for (var j = 0; j < itemCount; j++)
                                   {
                                        var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                                        indicatorDiagram.D.Add(Math.Round(value * 0.01, 3));
                                   }
                              }
                              else
                              {
                                   flag = false;

                                   indicatorDiagram.AlarmCode = -1;
                                   indicatorDiagram.AlarmMsg = "数据异常";

                                   indicatorDiagram.Mock = par.UseMockData;
                                   logIotModbusPoll.DateTime = DateTime.Now;
                                   logIotModbusPoll.State = -1;
                                   logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " +
                                                             itemCount.ToString() + " 个有线功图载荷数据异常!";

                                   redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll",
                                       logIotModbusPoll.ToJson().IndentJson());
                              }

                              Thread.Sleep(20);
                         }

                    }

                    if (flag)
                    {
                         var maxLoad = indicatorDiagram.D.Max();
                         var minLoad = indicatorDiagram.D.Min();

                         for (var i = 0; i < indicatorDiagram.D.Count; i++)
                         {
                              indicatorDiagram.D[i] = Math.Round(((indicatorDiagram.D[i] - minLoad) / (maxLoad - minLoad) * (double)indicatorDiagram.Displacement), 2);
                         }

                         if (indicatorDiagram.D.Count > 0)
                         {
                              indicatorDiagram.Displacement = Math.Round(indicatorDiagram.D.Max(), 2);
                              indicatorDiagram.MaxLoad = Math.Round(indicatorDiagram.L.Max(), 2);//最大载荷
                              indicatorDiagram.MinLoad = Math.Round(indicatorDiagram.L.Min(), 2);//最小载荷
                              indicatorDiagram.AvgLoad = Math.Round(indicatorDiagram.L.Average(), 2);//平均载荷
                         }
                    }

                    indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                    if (flag == true)
                    {
                         redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                         redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                         redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);
                    }

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, indicatorDiagram.ToJson().IndentJson());
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
