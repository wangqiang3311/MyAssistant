﻿using Acme.Common.Utils;
using HslCommunication.ModBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.WaterWell;

//浙江金华
namespace YCIOT.ModbusPoll.Vendor.ZJJH
{
    public static class Box
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_ZJJH_WTPM_TrunkPressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterStation = new IotDataWaterStation();
            var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

            var modbusAddress = par.ModbusAddress;
            ClientInfo.CurrentModbusPoolAddress = modbusAddress;

            waterStation.DeviceTypeId = par.DeviceTypeId;

            var flag = true;
            try
            {
                var read = await client.ReadAsync($"s={modbusAddress};x=3;220", 1);
                if (read.IsSuccess)
                {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    waterStation.StationId = par.DeviceId; //注水间ID   
                    waterStation.TrunkPressure = value / 100.0;//总汇管压力
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
                    logIotModbusPoll.Result = "读取注水间干压数据异常！";

                    redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }
                waterStation.NetworkNode = ClientInfo.ManyIpAddress;

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

        /// <summary>
        /// 读取配水间干压mock
        /// </summary>
        /// <param name="client"></param>
        /// <param name="redisClient"></param>
        /// <param name="messageString"></param>
        public static void Get_ZJJH_WTPM_TrunkPressure_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterStation = new IotDataWaterStation();

            waterStation.DeviceTypeId = par.DeviceTypeId;

            try
            {
                waterStation.AlarmCode = 0;
                waterStation.AlarmMsg = "mock数据";
                waterStation.StationId = par.DeviceId; //注水间ID   
                waterStation.Mock = par.UseMockData;


                waterStation.Mock = par.UseMockData;
                waterStation.DateTime = DateTime.Now;
                //用于将读取的结果写入Redis队列
                redisClient.AddItemToList("YCIOT:IOT_Data_WaterStation", waterStation.ToJson().IndentJson());
                redisClient.Set($"Group:WaterStation:{par.DeviceName}-{par.DeviceId}", waterStation);

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
        public static async Task Get_ZJJH_WII_WaterInjectingInstrument(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterWell = new IotDataWaterWell
            {
                AlarmCode = 0,
                AlarmMsg = ""
            };
            var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

            var modbusAddress = par.ModbusAddress;
            waterWell.DeviceTypeId = par.DeviceTypeId;
            ClientInfo.CurrentModbusPoolAddress = modbusAddress;

            try
            {
                if (!par.CommandParameter.IsNullOrEmpty())
                {
                    var dic = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                    if (dic != null)
                    {
                        var slotId = Convert.ToInt32(dic["1"]);

                        var flag = true;

                        var startAddress = (ushort)(1 + 6 * (slotId - 1));
                        var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", 6);
                        if (read.IsSuccess)
                        {
                            var value = client.ByteTransform.TransInt16(read.Content, 10);
                            waterWell.SettedFlow = value / 100.0; //设定流量回读

                            value = client.ByteTransform.TransInt16(read.Content, 4);

                            waterWell.InstantaneousFlow = value / 100.0; //瞬时流量

                            var value3 = client.ByteTransform.TransInt16(read.Content, 6);

                            var value4 = client.ByteTransform.TransInt16(read.Content, 8);

                            waterWell.CumulativeFlow = (value3 << 16 + value4) / 100.0;//表头累计

                            value = client.ByteTransform.TransInt16(read.Content, 0);

                            waterWell.ValveStatus = value; //阀门状态

                            value = client.ByteTransform.TransInt16(read.Content, 2);
                            waterWell.ValveMode = value; //阀门工作模式

                            waterWell.DateTime = DateTime.Now; //采集时间

                            //var result1 = await client.ReadHoldingRegisters(modbusAddress, (ushort)(30015 + 6 + 6 * (slotId - 1)), 1);
                            //waterWell.TubePressure = result1[0].Value / 100.0;//管压
                        }
                        else
                        {
                            flag = false;
                            waterWell.AlarmCode = -1;
                            waterWell.AlarmMsg = "数据异常";

                            waterWell.Mock = par.UseMockData;
                            logIotModbusPoll.DateTime = DateTime.Now;
                            logIotModbusPoll.State = -1;
                            logIotModbusPoll.Result = "读取注水井数据异常！";

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
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                Logger.Error(ex.Source);
            }
        }

        public static void Get_ZJJH_WII_WaterInjectingInstrument_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterWell = new IotDataWaterWell
            {
                AlarmCode = 0,
                AlarmMsg = ""
            };
            waterWell.DeviceTypeId = par.DeviceTypeId;

            try
            {
                waterWell.AlarmCode = -0;
                waterWell.AlarmMsg = "mock数据";
                waterWell.WellId = par.DeviceId;
                waterWell.Mock = par.UseMockData;
                waterWell.DateTime = DateTime.Now;

                redisClient.AddItemToList("YCIOT:IOT_Data_WaterWell", waterWell.ToJson().IndentJson());
                redisClient.Set($"Group:WaterWell:{par.DeviceName}-{par.DeviceId}", waterWell);


                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, waterWell.ToJson().IndentJson());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                Logger.Error(ex.Source);
            }
        }


        public static async Task Post_ZJJH_WII_InjectionAllocation(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterWell = new IotDataWaterWell();
            var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

            var modbusAddress = par.ModbusAddress;
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

                            var startAddress = (ushort)(5 + slotId - 1);

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
            }
        }

        public static async Task Post_ZJJH_WII_TubePressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var waterWell = new IotDataWaterWell();
            var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

            var modbusAddress = par.ModbusAddress;
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

                            //原始地址：40013
                            var startAddress = (ushort)(13 + slotId - 1);

                            var value = (ushort)(item.Value * 100);
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
            }
        }
    }
}
