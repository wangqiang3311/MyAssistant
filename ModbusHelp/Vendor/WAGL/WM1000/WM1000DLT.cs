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
    public static class WM1000DLT
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");


        public static async Task Get_XAGL_WM1000DLT_CurrentDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var currentDiagram = new IotDataOilWellCurrentDiagram()
                {
                    NetworkNode = ClientInfo.ManyIpAddress,
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                logIotModbusPoll.State = 0;
                logIotModbusPoll.Result = "ok";

                var modbusAddress = par.ModbusAddress;

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                currentDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                currentDiagram.D = new List<double>();  //位移
                currentDiagram.C = new List<double>(); //电流

                currentDiagram.DateTime = DateTime.Now;
                currentDiagram.WellId = par.DeviceId;

                currentDiagram.DeviceTypeId = par.DeviceTypeId;
                currentDiagram.Mock = false;

                var flag = true;
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
                        currentDiagram.NetworkNode = ClientInfo.ManyIpAddress;
                        currentDiagram.AlarmCode = 3;
                        currentDiagram.AlarmMsg = "停井";

                        redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_CurrentDiagram", currentDiagram.ToJson().IndentJson());
                        redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:CurrentDiagram", currentDiagram);
                        redisClient.Set($"Single:OilWell:CurrentDiagram:{par.DeviceName}-{par.DeviceId}", currentDiagram);

                        if (!par.UserName.IsNullOrEmpty())
                        {
                            ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0,
                                currentDiagram.ToJson().IndentJson());
                        }

                        return;
                    }
                }
                else
                {
                    flag = false;

                    currentDiagram.AlarmCode = -1;
                    currentDiagram.AlarmMsg = "数据异常";

                    currentDiagram.Mock = par.UseMockData;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = "读取采样间隔数据异常！";

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
                        currentDiagram.Count = 250;
                        currentDiagram.Interval = Math.Round(value * 0.01, 3);
                    }
                    else
                    {
                        flag = false;

                        currentDiagram.AlarmCode = -1;
                        currentDiagram.AlarmMsg = "数据异常";

                        logIotModbusPoll.Type = "Get_XAGL_WM1000DLT_CurrentDiagram";
                        logIotModbusPoll.DateTime = DateTime.Now;
                        currentDiagram.Mock = par.UseMockData;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = "读取采样间隔数据异常！";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                ushort step = 100;
                if (flag && currentDiagram.Count <= 300 && currentDiagram.Count >= step)
                {
                    var regAddress = 37768;//读取电流数据
                    for (ushort i = 0; i < currentDiagram.Count && flag; i += step)
                    {
                        var itemCount = (i + step > currentDiagram.Count) ? (ushort)(currentDiagram.Count - i) : step;
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
                            for (var j = 0; j < itemCount; j++)
                            {
                                var value = client.ByteTransform.TransInt16(read.Content, j++);
                                if (value != 0)
                                {
                                    var C = Math.Round(((value - 800) * 5 / 3200.0) * 15, 2);
                                    currentDiagram.C.Add(C);
                                }
                                else
                                {
                                    currentDiagram.C.Add(value);
                                }
                            }
                        }
                        else
                        {
                            flag = false;

                            currentDiagram.AlarmCode = -1;
                            currentDiagram.AlarmMsg = "数据异常";

                            currentDiagram.Mock = par.UseMockData;
                            logIotModbusPoll.Type = "Get_XAGL_WM1000DLT_CurrentDiagram";
                            logIotModbusPoll.DateTime = DateTime.Now;
                            logIotModbusPoll.State = -1;
                            logIotModbusPoll.Result = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个电流图电流数据异常!";

                            redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                        }

                        Thread.Sleep(100);
                    }

                    currentDiagram.MaxCurrent = currentDiagram.C.Max();//最大电流
                    currentDiagram.MinCurrent = currentDiagram.C.Min();//最小电流
                    currentDiagram.AvgCurrent = Math.Round(currentDiagram.C.Average(), 2);//平均电流
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
                        currentDiagram.D = DisplacementUtils.FitDisplacement((ushort)currentDiagram.Count,
                            (double)currentDiagram.Displacement);
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
    }
}
