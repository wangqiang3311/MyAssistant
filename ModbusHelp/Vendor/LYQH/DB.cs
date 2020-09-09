using Acme.Common.Utils;
using HslCommunication.ModBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ModbusPoll.Vendor.WAGL;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ModbusPoll.Vendor.LYQH
{
    /// <summary>
    /// 电参
    /// </summary>
    public class DB
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_LYQH_DB_PowerMeter(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var powerMeter = new IotDataOilWellPowerMeter()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                var modbusAddress = par.ModbusAddress;

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                var slotId = Convert.ToInt32(jo1["1"].ToString());
                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                powerMeter.DateTime = DateTime.Now;
                powerMeter.WellId = par.DeviceId;

                powerMeter.DeviceTypeId = par.DeviceTypeId;
                powerMeter.Mock = false;

                var startAddress = (ushort)((slotId * 0x3000) + 1606);

                var flag = true;

                var isSuccess = await GetPowerMeter(client, modbusAddress, powerMeter, startAddress, 9, 3);

                if (!isSuccess)
                {
                    flag = false;
                    FailLog.Write(redisClient, par, powerMeter, logIotModbusPoll, "Get_LYQH_DB_PowerMeter", "读取电参数据异常！");
                }

                if (flag)
                {
                    //常规时间
                    startAddress = (ushort)((slotId * 0x3000) + 1615);
                    isSuccess = await GetDateTime(client, modbusAddress, powerMeter, startAddress, 3, 3);

                    if (!isSuccess)
                    {
                        FailLog.Write(redisClient, par, powerMeter, logIotModbusPoll, "Get_LYQH_DB_PowerMeter", "读取常规时间数据异常！");
                    }
                }

                powerMeter.NetworkNode = ClientInfo.ManyIpAddress;

                //用于将读取的结果写入Redis队列 
                if (isSuccess || par.UseMockData)
                {
                    powerMeter.Mock = par.UseMockData;
                    redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_PowerMeter", powerMeter.ToJson().IndentJson());
                    redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:PowerMeter", powerMeter);
                    redisClient.Set($"Single:OilWell:PowerMeter:{par.DeviceName}-{par.DeviceId}", powerMeter);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, !isSuccess ? -2 : 0, powerMeter.ToJson().IndentJson());
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


        public static async Task<bool> GetPowerMeter(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPowerMeter powerMeter, ushort startAddress, ushort regCount, int tryReadTimes)
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
                var value = client.ByteTransform.TransInt16(read.Content, 0);

                // A相电压  1606  变比 0.1
                powerMeter.APhaseVoltage = Math.Round(value * 0.1, 1);

                value = client.ByteTransform.TransInt16(read.Content, 2);

                // B相电压  1607  变比 0.1
                powerMeter.BPhaseVoltage = Math.Round(value * 0.1, 1);

                value = client.ByteTransform.TransInt16(read.Content, 4);

                // C相电压  1608  变比 0.1
                powerMeter.CPhaseVoltage = Math.Round(value * 0.1, 1);

                value = client.ByteTransform.TransInt16(read.Content, 6);

                // A相电流  1609  变比 0.1
                powerMeter.APhaseCurrent = Math.Round(value * 0.01, 1);

                value = client.ByteTransform.TransInt16(read.Content, 8);
                // B相电流   1610  变比 0.1
                powerMeter.BPhaseCurrent = Math.Round(value * 0.01, 1);

                value = client.ByteTransform.TransInt16(read.Content, 10);

                // C相电流   1611  变比 0.1
                powerMeter.CPhaseCurrent = Math.Round(value * 0.01, 1);

                value = client.ByteTransform.TransInt16(read.Content, 12);
                //当前有功功率  1612  变比 0.01
                var currentYGGL = Math.Round(value * 0.01, 2);

                Console.WriteLine("当前有功功率:" + currentYGGL);

                value = client.ByteTransform.TransInt16(read.Content, 14);
                //当前总功率  1613  变比 0.01
                powerMeter.TotalActivePower = Math.Round(value * 0.01, 2);

                value = client.ByteTransform.TransInt16(read.Content, 16);
                //当前功率因数 1614  变比 1
                var currentGLFactor = value;
                powerMeter.TotalPowerFactor = currentGLFactor;
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetPowerMeter(client, modbusAddress, powerMeter, startAddress, regCount, tryReadTimes);
                }
                powerMeter.AlarmMsg = read.Message;
                return false;
            }
        }


        public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellPowerMeter powerMeter, ushort startAddress, ushort regCount, int tryReadTimes)
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

                powerMeter.DateTime = new DateTime(2000 + year, month, date, h, m, s);

                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDateTime(client, modbusAddress, powerMeter, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }


        public static async Task POST_LYQH_DB_ShiShiCaiJI(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                indicatorDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                var slotId = Convert.ToInt32(jo1["1"].ToString());

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                var startAddress = (ushort)((slotId * 0x100) + 0x0090);

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 00, 01, 20, 08, 27, 16, 44, 0 });

                var flag = true;
                if (read.IsSuccess)
                {


                }
                else
                {
                    flag = false;
                    FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", "实时采集异常！");
                }


                indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true || par.UseMockData)
                {
                    indicatorDiagram.Mock = par.UseMockData;

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


        public static async Task POST_LYQH_DBSetting(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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


                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                var startAddress = (ushort)0x0640;

                /*
                     仪表编号：2020010001
                     采集周期：1800
                     采集模式：0x0
                     modbus地址：3
                     modbus槽位：0
                     点数 500
                     电参模式 0
                     12345678
                     1000
                */

                // ushort[] data = { 0x20, 59409, 30822, 3, 0, 1800, 0, 0x00, 500, 513, 1027, 1541, 2055, 1000 };

                byte[] macAddress = { 0x21, 0x94, 0x50, 0xFE, 0xFF, 0x14, 0x2E, 0x84 };

                var data = GetSettingData(2020000005, 4, 1, 1800, 0x0, 500, macAddress, 2000);


                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", data);

                var flag = true;
                if (read.IsSuccess)
                {


                }
                else
                {
                    flag = false;
                    FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", "实时采集异常！");
                }


                indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true || par.UseMockData)
                {
                    indicatorDiagram.Mock = par.UseMockData;
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


        public static ushort[] GetSettingData(int id, ushort modbusAddress, ushort slot, ushort period, ushort mode, ushort count, byte[] macAddress, ushort k)
        {
            var idByte = GT.TransIdToByteByMoveBit(id);

            var idData = GT.GetIDData(idByte);

            var address = GT.GetMACData(macAddress);

            ushort[] data = { 0x20, idData.Item1, idData.Item2, modbusAddress, slot, period, 0, mode, count, address.Item1, address.Item2, address.Item3, address.Item4, k };

            return data;
        }
    }
}
