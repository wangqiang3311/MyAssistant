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
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

namespace YCIOT.ModbusPoll.Vendor.WAGL.WM2000
{
    public static class WM2000GLT
    {
        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_XAGL_WM2000GLT_PowerDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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


                #region  判断井状态

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 2;
                }

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4103", 1);
                if (read.IsSuccess)
                {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);

                    if (value == 3)  //1：正转运行；3：变频停机
                    {
                        powerDiagram.NetworkNode = ClientInfo.ManyIpAddress;
                        powerDiagram.AlarmCode = 3;
                        powerDiagram.AlarmMsg = "停井";
                    }
                }
                else
                {
                    flag = false;
                    powerDiagram.AlarmCode = -1;
                    powerDiagram.AlarmMsg = "数据异常";

                    powerDiagram.Mock = par.UseMockData;
                    logIotModbusPoll.DateTime = DateTime.Now;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取功率图井状态数据异常！[{read.Message}]";
                    $"GLT:{par.DeviceName}-{par.DeviceId}{logIotModbusPoll.Result}".Info();

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }
                #endregion

                const ushort step = 100;

                //ToDo:确认采样间隔和点数
                powerDiagram.Count = 300;

                if (flag && powerDiagram.Count <= 300 && powerDiagram.Count >= step)
                {

                    const ushort regAddress = 38268; //读取功率数据
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
                            logIotModbusPoll.Type = "Get_XAGL_WM2000GLT_PowerDiagram";
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

        public static void Get_XAGL_WM2000GLT_PowerDiagram_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var powerDiagram = new IotDataOilWellPowerDiagram()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                powerDiagram.WellId = par.DeviceId;
                powerDiagram.DeviceTypeId = par.DeviceTypeId;
                powerDiagram.DateTime = DateTime.Now;
                powerDiagram.Mock = par.UseMockData;

                redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_PowerDiagram", powerDiagram.ToJson().IndentJson());
                redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:PowerDiagram", powerDiagram);
                redisClient.Set($"Single:OilWell:PowerDiagram:{par.DeviceName}-{par.DeviceId}", powerDiagram);
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
