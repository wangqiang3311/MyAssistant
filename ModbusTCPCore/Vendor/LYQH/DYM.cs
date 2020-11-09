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
using YCIOT.ModbusPoll.Vendor.WAGL;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

namespace YCIOT.ModbusPoll.Vendor.LYQH
{
    /// <summary>
    /// 动液面
    /// </summary>
    public class DYM
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static async Task Get_LYQH_DYM_Movefluidlevel(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                IotDataOilWellMovefluidlevel dytDiagram = new IotDataOilWellMovefluidlevel
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                logIotModbusPoll.State = 0;
                logIotModbusPoll.Result = "ok";

                var modbusAddress = par.ModbusAddress;
                dytDiagram.DeviceTypeId = par.DeviceTypeId;
                dytDiagram.Mock = false;

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);

                var slotId = Convert.ToInt32(jo1["1"].ToString());
                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                dytDiagram.Y = new List<double>();  //位移

                dytDiagram.DateTime = DateTime.Now;
                dytDiagram.WellId = par.DeviceId;
                dytDiagram.MovefluidHeight = 600;

                var flag = await SetdytDiagram(redisClient, client, modbusAddress, dytDiagram, logIotModbusPoll, par, slotId);

                dytDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true || par.UseMockData)
                {
                    dytDiagram.Mock = par.UseMockData;

                    redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_Movefluidlevel", dytDiagram.ToJson().IndentJson());
                    redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:Movefluidlevel", dytDiagram);
                    redisClient.Set($"Single:OilWell:Movefluidlevel:{par.DeviceName}-{par.DeviceId}", dytDiagram);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, dytDiagram.ToJson().IndentJson());
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

        public static async Task<bool> SetdytDiagram(RedisClient redisClient, ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, LogIotModbusPoll logIotModbusPoll, ControlRequest par, int slotId, int tryReadTimes = 3)
        {
            var regAddress = (ushort)((slotId * 0x3000) + 1536);  //读取位移数据
            var isSuccess = await GetAll(client, modbusAddress, dytDiagram, regAddress, 6, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, dytDiagram, logIotModbusPoll, "Get_LYQH_DYM_Movefluidlevel", "读取动液面异常！");
                return false;
            }

            ushort step = 100;

            //读取序列数据
            if (dytDiagram.Count <= 6000 && dytDiagram.Count >= step)
            {
                regAddress = (ushort)((slotId * 0x3000) + 1636);  //读取序列数据
                for (var i = 0; i < dytDiagram.Count; i += step)
                {
                    var itemCount = (i + step > dytDiagram.Count) ? (ushort)(dytDiagram.Count - i) : step;

                    Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                    //读取序列
                    isSuccess = await GetDData(client, modbusAddress, dytDiagram, (ushort)(regAddress + i), itemCount, tryReadTimes);

                    if (!isSuccess)
                    {
                        var message = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个动液面位移数据异常!";
                        FailLog.Write(redisClient, par, dytDiagram, logIotModbusPoll, "Get_LYQH_DYM_Movefluidlevel", message);
                        return false;
                    }
                    Thread.Sleep(100);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes = 3)
        {
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);
            if (read.IsSuccess)
            {
                var year = client.ByteTransform.TransUInt16(read.Content, 0);
                var month = client.ByteTransform.TransUInt16(read.Content, 1);
                var date = client.ByteTransform.TransUInt16(read.Content, 2);
                var h = client.ByteTransform.TransUInt16(read.Content, 3);
                var m = client.ByteTransform.TransUInt16(read.Content, 4);
                var s = client.ByteTransform.TransUInt16(read.Content, 5);

                dytDiagram.DateTime = new DateTime(2000 + year, month, date, h, m, s);
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDateTime(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetDData(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                    //无符号数0-65535
                    var value = client.ByteTransform.TransUInt16(read.Content, j * 2);
                    if (value != 0)
                    {
                        var L = value;
                        dytDiagram.Y.Add(value);
                    }
                    else
                    {
                        dytDiagram.Y.Add(value);
                    }
                }
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDData(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }
        public static async Task<bool> GetMode(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
        {
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);

            if (read.IsSuccess)
            {
                var value = client.ByteTransform.TransInt16(read.Content, 0);
                dytDiagram.Model = value;
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetMode(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetCount(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
        {
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);

            if (read.IsSuccess)
            {
                var value = client.ByteTransform.TransInt16(read.Content, 0);
                dytDiagram.Count = value;
                return true;
            }
            else
            {
                tryReadTimes--;

                if (tryReadTimes > 0)
                {
                    return await GetCount(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }

                return false;
            }
        }

        public static async Task<bool> GetPeriod(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
        {
            var read = await client.ReadAsync($"s={modbusAddress};x=4;{startAddress}", regCount);

            if (read.IsSuccess)
            {
                var value = client.ByteTransform.TransInt16(read.Content, 0);
                dytDiagram.Count = value;
                return true;
            }
            else
            {
                tryReadTimes--;

                if (tryReadTimes > 0)
                {
                    return await GetPeriod(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }

                return false;
            }
        }

        public static async Task<bool> GetAll(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellMovefluidlevel dytDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                //读取时间
                var year = client.ByteTransform.TransByte(read.Content, 0);
                var month = client.ByteTransform.TransByte(read.Content, 1);
                var date = client.ByteTransform.TransByte(read.Content, 2);
                var h = client.ByteTransform.TransByte(read.Content, 3);
                var m = client.ByteTransform.TransByte(read.Content, 4);
                var s = client.ByteTransform.TransByte(read.Content, 5);

                dytDiagram.DateTime = new DateTime(2000 + year, month, date, h, m, s);

                //采样周期
                var value = client.ByteTransform.TransInt16(read.Content, 6);

                dytDiagram.Period = (int)(value * 0.01);

                //模式
                value = client.ByteTransform.TransInt16(read.Content, 8);

                dytDiagram.Model = value;

                //点数
                value = client.ByteTransform.TransInt16(read.Content, 10);

                dytDiagram.Count = value;
                return true;
            }
            else
            {
                tryReadTimes--;

                if (tryReadTimes > 0)
                {
                    return await GetAll(client, modbusAddress, dytDiagram, startAddress, regCount, tryReadTimes);
                }

                return false;
            }
        }

        public static async Task POST_LYQH_DYMShiShiCaiJI(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var startAddress = (ushort)((slotId * 0x100) + 0x0070);

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 00, 01, 20, 08, 27, 17, 10, 0 });

                var flag = true;
                if (read.IsSuccess)
                {


                }
                else
                {
                    flag = false;
                    FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "POST_LYQH_DYMShiShiCaiJI", "动液面实时采集异常！");
                }


                indicatorDiagram.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true || par.UseMockData)
                {
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


        public static async Task POST_LYQH_DYMSetting(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var startAddress = (ushort)0x0C80;

                //写入10个寄存器
                /*
                    仪表类型：0x30
                    仪表编号：1801064
                    modbus地址：5
                    modbus槽位：1
                    采集周期：1800
                    预估深度：30000
                    预设声速：3400
                    接箍长度：9600
                    采样周期：400
                    电磁阀充电时间：10
                    采集模式：0x0
                    球阀动作时间：9
                    电磁阀动作时间：4
                    64位长地址：F2 98 50 FE FF 14 2E 84
                */

                byte[] macAddress = { 0xF2, 0x98, 0x50, 0xFE, 0xFF, 0x14, 0x2E, 0x84 };
                var data = GetSettingData(1801064, 5, 1, 1800, 30000, 3400, 9600, 400, 10, 0x0, 9, 4, macAddress);
                //ushort[] data = { 0x30, 9088, 30748, 1, 1, 1800, 0, 30000, 3400, 9600, 400, 10, 0, 10, 20, 41130, 65140, 22527, 11 };

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

        public static ushort[] GetSettingData(int id, ushort modbusAddress, ushort slot, ushort period, ushort deep,

         ushort sound, ushort length, ushort cPeriod, ushort time, ushort mode, ushort actionTime, ushort action1Time, byte[] macAddress)
        {
            var idByte = GT.TransIdToByteByMoveBit(id);

            var idData = GT.GetIDData(idByte);

            var address = GT.GetMACData(macAddress);

            ushort[] data = { 0x30, idData.Item1, idData.Item2, modbusAddress, slot, period, 0, deep,sound, length, cPeriod,
              time,  mode,actionTime,action1Time, address.Item1, address.Item2, address.Item3, address.Item4};


            //ushort[] data = { 0x30, 9088, 30748, 1, 1, 1800, 0, 30000, 3400, 9600, 400, 10, 0, 10, 20, 41130, 65140, 22527, 11 };

            return data;
        }

    }
}

