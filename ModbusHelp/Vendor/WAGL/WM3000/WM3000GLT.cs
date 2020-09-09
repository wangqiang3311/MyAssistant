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
    public static class WM3000GLT
    {
        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_XAGL_WM3000GLT_PowerDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                powerDiagram.D = new List<double>();  //位移
                powerDiagram.P = new List<double>(); //功率

                powerDiagram.DateTime = DateTime.Now;
                powerDiagram.WellId = par.DeviceId;


                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 2;
                }

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4103", 1);

                if (!read.IsSuccess)
                {
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4103", 1);
                }

                if (read.IsSuccess)
                {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    if (value == 3)
                    {
                        powerDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                        powerDiagram.AlarmCode = 3;
                        powerDiagram.AlarmMsg = "停井";

                        redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_PowerDiagram",
                            powerDiagram.ToJson().IndentJson());
                        redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:PowerDiagram", powerDiagram);


                        if (!par.UserName.IsNullOrEmpty())
                        {
                            ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0,
                                powerDiagram.ToJson().IndentJson());
                        }

                        return;
                    }
                }
                else
                {
                    flag = false;

                    powerDiagram.AlarmCode = -1;
                    powerDiagram.AlarmMsg = "数据异常";

                    logIotModbusPoll.Type = "Get_XAGL_WM3000GLT_PowerDiagram";
                    logIotModbusPoll.DateTime = DateTime.Now;
                    powerDiagram.Mock = par.UseMockData;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取采样间隔数据异常！[{read.Message}]";
                    $"GLT:{par.DeviceName}-{par.DeviceId}{logIotModbusPoll.Result}".Info();

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
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4626", 1);

                    if (!read.IsSuccess)
                    {
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4626", 1);
                    }

                    if (read.IsSuccess)
                    {
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        powerDiagram.Interval = Math.Round(value * 0.01, 3);
                    }
                    else
                    {
                        flag = false;

                        powerDiagram.AlarmCode = -1;
                        powerDiagram.AlarmMsg = "数据异常";

                        logIotModbusPoll.Type = "Get_XAGL_WM3000GLT_PowerDiagram";
                        logIotModbusPoll.DateTime = DateTime.Now;

                        powerDiagram.Mock = par.UseMockData;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取采样间隔数据异常！[{read.Message}]";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 2;
                    }
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;6697", 1);

                    if (!read.IsSuccess)
                    {
                        read = await client.ReadAsync($"s={par.ModbusAddress};x=3;6697", 1);
                    }

                    if (read.IsSuccess)
                    {
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        powerDiagram.Count = value;
                    }
                    else
                    {
                        flag = false;

                        powerDiagram.AlarmCode = -1;
                        powerDiagram.AlarmMsg = "数据异常";

                        powerDiagram.Mock = par.UseMockData;
                        logIotModbusPoll.Type = "Get_XAGL_WM3000GLT_PowerDiagram";
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取采样点数数据异常！{read.Message}";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                const ushort step = 100;
                if (flag && powerDiagram.Count <= 300 && powerDiagram.Count >= step)
                {

                    const ushort regAddress = 38269; //读取功率数据
                    for (ushort i = 0; i < powerDiagram.Count && flag; i += step)
                    {
                        var itemCount = (i + step > powerDiagram.Count) ? (ushort)(powerDiagram.Count - i) : step;
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
                                var P = Math.Round((value / 4000.0) * 36, 2);
                                powerDiagram.P.Add(P);
                            }
                        }
                        else
                        {
                            flag = false;

                            powerDiagram.AlarmCode = -1;
                            powerDiagram.AlarmMsg = "数据异常";

                            powerDiagram.Mock = par.UseMockData;
                            logIotModbusPoll.Type = "Get_XAGL_WM3000GLT_PowerDiagram";
                            logIotModbusPoll.DateTime = DateTime.Now;
                            logIotModbusPoll.State = -1;
                            logIotModbusPoll.Result =
                                "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() +
                                $" 个功率图功率数据异常!{read.Message}";

                            redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
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
                    $"GLT:{par.DeviceName}-{par.DeviceId}已获取到数据".Info();
                    powerDiagram.Mock = par.UseMockData;

                    if (powerDiagram.Count != null)
                    {
                        if (powerDiagram.Displacement != null)
                        {
                            powerDiagram.D =
                                DisplacementUtils.FitDisplacement((ushort)powerDiagram.Count,
                                    (double)powerDiagram.Displacement);
                        }
                    }

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
    }
}
