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
using NLog.Fluent;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ModbusPoll.Vendor.WAGL;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilOmeter;
using YCIOT.ServiceModel.OilWell;

/// <summary>
/// 液位罐计量
/// </summary>
namespace YCIOT.ModbusPoll.Vendor.LYQH
{
    public static class GJL
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_LYQH_GJL_Tankmeasure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var tankmeasure = new IotDataOilOmeter()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                tankmeasure.DateTime = DateTime.Now;
                tankmeasure.OilOmeterId = par.DeviceId;

                tankmeasure.DeviceTypeId = par.DeviceTypeId;
                tankmeasure.Mock = false;

                //从罐Id中获取大罐编号(0-3),此编号是协议中定的
                var gId = int.Parse(par.DeviceId.ToString().Last().ToString());

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);
                var slotId = Convert.ToInt32(jo1["1"].ToString());

                var startAddress = (ushort)(slotId * 0x600 + gId * 16);

                var isSuccess = await SetGJL(redisClient, client, modbusAddress, startAddress, tankmeasure, logIotModbusPoll, par);

                tankmeasure.NetworkNode = ClientInfo.ManyIpAddress;

                //用于将读取的结果写入Redis队列 
                if (isSuccess || par.UseMockData)
                {
                    tankmeasure.Mock = par.UseMockData;
                    redisClient.AddItemToList("YCIOT:IOT_Data_WellField_Tankmeasure", tankmeasure.ToJson().IndentJson());
                    redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:Tankmeasure", tankmeasure);
                    redisClient.Set($"Single:OilWell:Tankmeasure:{par.DeviceName}-{par.DeviceId}", tankmeasure);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, isSuccess ? 0 : -2, tankmeasure.ToJson().IndentJson());
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

        public static async Task<bool> GetGJL(ModbusRtuOverTcp client, int modbusAddress, IotDataOilOmeter tankmeasure, ushort startAddress, ushort regCount, int tryReadTimes)
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

                //毫米
                tankmeasure.CurrentElevation = value;
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetGJL(client, modbusAddress, tankmeasure, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilOmeter tankmeasure, ushort startAddress, ushort regCount, int tryReadTimes)
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

                if (year != 0 && month != 0 && date != 0)
                {
                    tankmeasure.DateTime = new DateTime(2000 + year, month, date, h, m, s);
                }
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDateTime(client, modbusAddress, tankmeasure, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> SetGJL(RedisClient redisClient, ModbusRtuOverTcp client, int modbusAddress, ushort startAddress, IotDataOilOmeter tankmeasure, LogIotModbusPoll logIotModbusPoll, ControlRequest par, int tryReadTimes = 3)
        {
            //获取采集时间

            var address = startAddress + 1;
            var isSuccess = await GetDateTime(client, modbusAddress, tankmeasure, (ushort)address, 3, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, tankmeasure, logIotModbusPoll, "Get_LYQH_GJL_Tankmeasure", "读取液位罐计量时间数据异常！");
                return false;
            }

            isSuccess = await GetGJL(client, modbusAddress, tankmeasure, startAddress, 1, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, tankmeasure, logIotModbusPoll, "Get_LYQH_GJL_Tankmeasure", "读取液位罐计量数据异常！");
                return false;
            }
            return true;
        }


        public static async Task POST_LYQH_GJLSetting(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var startAddress = (ushort)0x5140;

                /*
                    仪表类型：0x70
                    仪表编号：2020010002
                    modbus地址：240
                    modbus槽位：0
                    采集周期：1800
                    液位模式：0
                    校正K: 106
                    零偏B：0
                    井场安装编号：3
                    64位长地址：86 92 50 FE FF 14 2E 84
                */

                byte[] macAddress = { 0x86, 0x92, 0x50, 0xFE, 0xFF, 0x14, 0x2E, 0x84 };

                var data = GetSettingData(2020010002, 240, 0, 1800, 0, 106, 0, 3, macAddress);

                //ushort[] data = { 0x70, 9088, 30748, 1, 1, 400, 0, 0x00, 1, 1, 1, 513, 1027, 1541, 2055 };

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


        public static async Task POST_LYQH_GJLShiShiCaiJI(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var startAddress = (ushort)((slotId * 0x100) + 0x0030);

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 00, 01, 20, 08, 27, 17, 21, 0 });

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



        public static ushort[] GetSettingData(int id, ushort modbusAddress, ushort slot, ushort period, ushort mode, ushort k, ushort b, ushort n, byte[] macAddress)
        {
            var idByte = GT.TransIdToByteByMoveBit(id);

            var idData = GT.GetIDData(idByte);

            var address = GT.GetMACData(macAddress);

            //ushort[] data = { 0x70, 9088, 30748, 1, 1, 400, 0, 0x00, 1, 1, 1, 513, 1027, 1541, 2055 };
            ushort[] data = { 0x70, idData.Item1, idData.Item2, modbusAddress, slot, period, 0, mode, k, b, n, address.Item1, address.Item2, address.Item3, address.Item4 };

            return data;
        }
    }
}