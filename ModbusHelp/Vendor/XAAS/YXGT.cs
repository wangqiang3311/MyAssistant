using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ServiceModel;
using Acme.Common.Utils;
using ServiceStack.Configuration;
using YCIOT.ServiceModel.OilWell;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.XAAS
{
    // ReSharper disable once InconsistentNaming
    public static class YXGT
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");

        public static async Task Get_XAAS_WII_IndicatorDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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
                if (jo1["0"] != null)
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
                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;40532", 1);

                if (!read.IsSuccess)
                {
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;40532", 1);
                }

                if (read.IsSuccess)
                {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    if (value == 1)
                    {
                        indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                        indicatorDiagram.AlarmCode = 1;
                        indicatorDiagram.AlarmMsg = "停井";

                        redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                        redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);

                        if (!par.UserName.IsNullOrEmpty())
                        {
                            ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, indicatorDiagram.ToJson().IndentJson());
                        }

                        return;
                    }
                }
                else
                {
                    flag = false;

                    indicatorDiagram.AlarmCode = -1;
                    indicatorDiagram.AlarmMsg = "数据异常";

                    logIotModbusPoll.Type = "Get_XAAS_IndicatorDiagram";
                    logIotModbusPoll.DateTime = DateTime.Now;
                    indicatorDiagram.Mock = par.UseMockData;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取采样间隔数据异常！[{read.Message}]";
                    $"YXGT:{par.DeviceName}-{par.DeviceId}{ logIotModbusPoll.Result}".Info();

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());

                }
                #region 读取采样点数
                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 2;
                    }
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;41488", 1);

                    if (!read.IsSuccess)
                    {
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;41488", 1);
                    }

                    if (read.IsSuccess)
                    {
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        indicatorDiagram.Count = value;
                    }
                    else
                    {
                        flag = false;

                        indicatorDiagram.AlarmCode = -1;
                        indicatorDiagram.AlarmMsg = "数据异常";

                        indicatorDiagram.Mock = par.UseMockData;
                        logIotModbusPoll.Type = "Get_XAAS_IndicatorDiagram";
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取采样点数数据异常！[{read.Message}]";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }
                #endregion

                #region  读取冲次和冲程

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 4;
                    }
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;41493", 2);

                    if (!read.IsSuccess)
                    {
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;41493", 2);
                    }

                    if (read.IsSuccess)
                    {
                        //变比0.01
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        indicatorDiagram.Stroke = value * 0.01;

                        value = client.ByteTransform.TransInt16(read.Content, 2);
                        indicatorDiagram.Displacement = value * 0.01;

                    }
                    else
                    {
                        flag = false;

                        indicatorDiagram.AlarmCode = -1;
                        indicatorDiagram.AlarmMsg = "数据异常";

                        indicatorDiagram.Mock = par.UseMockData;
                        logIotModbusPoll.Type = "Get_XAAS_IndicatorDiagram";
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取冲程冲次数据异常！[{read.Message}]";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                #endregion


                const ushort step = 100;
                if (flag && indicatorDiagram.Count <= 400 && indicatorDiagram.Count >= step)
                {
                    #region  读取位移数据

                    ushort regAddress = 41499;
                    for (var i = 0; i < indicatorDiagram.Count && flag; i += step)
                    {
                        var itemCount = (i + step > indicatorDiagram.Count) ? (ushort)(indicatorDiagram.Count - i) : step;
                        if (isDebug)
                            Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                        lock (ClientInfo.locker)
                        {
                            ClientInfo.RequestTime = DateTime.Now;
                            ClientInfo.ExpectedType = 0x03;
                            ClientInfo.ExpectedDataLen = itemCount * 2;
                        }
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;{(regAddress + i)}", itemCount);

                        if (!read.IsSuccess)
                        {
                            read = await client.ReadAsync($"s={par.ModbusAddress};x=3;{(regAddress + i)}", itemCount);
                        }

                        if (read.IsSuccess)
                        {
                            for (var j = 0; j < itemCount; j++)
                            {
                                var value = client.ByteTransform.TransInt16(read.Content, j * 2);

                                //变比0.01
                                indicatorDiagram.D.Add(value * 0.01);
                            }
                        }
                        else
                        {
                            flag = false;

                            indicatorDiagram.AlarmCode = -1;
                            indicatorDiagram.AlarmMsg = "数据异常";

                            indicatorDiagram.Mock = par.UseMockData;
                            logIotModbusPoll.Type = "Get_XAAS_IndicatorDiagram";
                            logIotModbusPoll.DateTime = DateTime.Now;
                            logIotModbusPoll.State = -1;
                            logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " +
                                                      itemCount.ToString() + $" 个有线功图位移数据异常![{read.Message}]";

                            redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll",
                                logIotModbusPoll.ToJson().IndentJson());
                        }

                        Thread.Sleep(100);
                    }

                    #endregion

                    #region  读取载荷数据

                    regAddress = 41899;

                    for (ushort i = 0; i < indicatorDiagram.Count && flag; i += step)
                    {
                        var itemCount = (i + step > indicatorDiagram.Count) ? (ushort)(indicatorDiagram.Count - i) : step;
                        if (isDebug)
                            Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                        lock (ClientInfo.locker)
                        {
                            ClientInfo.RequestTime = DateTime.Now;
                            ClientInfo.ExpectedType = 0x03;
                            ClientInfo.ExpectedDataLen = itemCount * 2;
                        }
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;{(regAddress + i)}", itemCount);

                        if (!read.IsSuccess)
                        {
                            read = await client.ReadAsync($"s={par.ModbusAddress};x=3;{(regAddress + i)}", itemCount);
                        }

                        if (read.IsSuccess)
                        {
                            for (var j = 0; j < itemCount; j++)
                            {
                                var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                                if (value != 0)
                                {
                                    //变比0.01
                                    var Load = (value * 0.01 - 800) * 150 / 3200.0;
                                    indicatorDiagram.L.Add(Math.Round(Load, 2));
                                }
                                else
                                {
                                    indicatorDiagram.L.Add(value);
                                }
                            }
                        }
                        else
                        {
                            flag = false;

                            indicatorDiagram.AlarmCode = -1;
                            indicatorDiagram.AlarmMsg = "数据异常";

                            indicatorDiagram.Mock = par.UseMockData;
                            logIotModbusPoll.Type = "Get_XAAS_IndicatorDiagram";
                            logIotModbusPoll.DateTime = DateTime.Now;
                            logIotModbusPoll.State = -1;
                            logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + $" 个有线功图载荷数据异常![{read.Message}]";

                            redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                        }
                        Thread.Sleep(100);
                    }

                    #endregion

                    //读取位移数据

                    var maxDis = indicatorDiagram.D.Max();
                    var minDis = indicatorDiagram.D.Min();

                    if (!indicatorDiagram.Displacement.HasValue)
                    {
                        indicatorDiagram.Displacement = maxDis;
                    }

                    for (var i = 0; i < indicatorDiagram.D.Count; i++)
                    {
                        if (Math.Abs(maxDis - minDis) > 0.1)
                        {
                            indicatorDiagram.D[i] = Math.Round(((indicatorDiagram.D[i] - minDis) / (maxDis - minDis) * (double)indicatorDiagram.Displacement), 2);
                        }
                    }

                    if (indicatorDiagram.D.Count > 0)
                    {
                        indicatorDiagram.MaxLoad = Math.Round(indicatorDiagram.L.Max(), 2);//最大载荷
                        indicatorDiagram.MinLoad = Math.Round(indicatorDiagram.L.Min(), 2);//最小载荷
                        indicatorDiagram.AvgLoad = Math.Round(indicatorDiagram.L.Average(), 2);//平均载荷
                        indicatorDiagram.D.Add(indicatorDiagram.D[0]);
                        indicatorDiagram.L.Add(indicatorDiagram.L[0]);
                    }
                }
                else
                {
                    flag = false;
                }

                indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true)
                {
                    $"YXGT:{par.DeviceName}-{par.DeviceId}已获取到数据".Info();
                    indicatorDiagram.Mock = par.UseMockData;

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

        public static void Get_XAAS_IndicatorDiagram_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var indicatorDiagram = new IotDataOilWellIndicatorDiagram()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                indicatorDiagram.WellId = par.DeviceId;
                indicatorDiagram.DeviceTypeId = par.DeviceTypeId;
                indicatorDiagram.DateTime = DateTime.Now;
                indicatorDiagram.Mock = par.UseMockData;

                redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);
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
