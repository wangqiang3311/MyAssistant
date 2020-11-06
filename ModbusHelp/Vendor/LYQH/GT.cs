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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
    /// <summary>
    /// 功图
    /// </summary>
    public static class GT
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static async Task Get_LYQH_WG_IndicatorDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                indicatorDiagram.D = new List<double>();  //位移
                indicatorDiagram.L = new List<double>();  //载荷

                indicatorDiagram.DateTime = DateTime.Now;
                indicatorDiagram.WellId = par.DeviceId;

                var flag = await SetIndicatorDiagram(redisClient, client, modbusAddress, indicatorDiagram, logIotModbusPoll, par, slotId);

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

        public static async Task<bool> SetIndicatorDiagram(RedisClient redisClient, ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, LogIotModbusPoll logIotModbusPoll, ControlRequest par, int slotId, int tryReadTimes = 3)
        {
            var startAddress = (ushort)((slotId * 0x3000) + 256);

            //时间
            var isSuccess = await GetDateTime(client, modbusAddress, indicatorDiagram, startAddress, 3, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", "读取采样时间数据异常！");
                return false;
            }

            startAddress = (ushort)((slotId * 0x3000) + 260);
            //采样点数
            isSuccess = await GetCount(client, modbusAddress, indicatorDiagram, startAddress, 1, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", "读取采样点数数据异常！");
                return false;
            }

            startAddress = (ushort)((slotId * 0x3000) + 263);

            //读取冲次
            isSuccess = await GetStroke(client, modbusAddress, indicatorDiagram, startAddress, 1, tryReadTimes);

            if (!isSuccess)
            {
                FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", "读取冲次异常！");
                return false;
            }

            ushort step = 100;

            if (indicatorDiagram.Count <= 250 && indicatorDiagram.Count >= step)
            {
                var regAddress = (ushort)((slotId * 0x3000) + 606);  //读取载荷数据

                for (ushort i = 0; i < indicatorDiagram.Count; i += step)
                {
                    var itemCount = (i + step > indicatorDiagram.Count) ? (ushort)(indicatorDiagram.Count - i) : step;

                    Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                    //读取载荷
                    isSuccess = await GetLData(client, modbusAddress, indicatorDiagram, (ushort)(regAddress + i), itemCount, tryReadTimes);

                    if (!isSuccess)
                    {
                        var message = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个载荷数据异常!";
                        FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", message);
                        return false;
                    }
                    Thread.Sleep(100);
                }

                regAddress = (ushort)((slotId * 0x3000) + 356);  //读取位移数据

                for (var i = 0; i < indicatorDiagram.Count; i += step)
                {
                    var itemCount = (i + step > indicatorDiagram.Count) ? (ushort)(indicatorDiagram.Count - i) : step;

                    Logger.Info($"{i}:{itemCount}:{(ushort)(regAddress + i)}");

                    //读取位移
                    isSuccess = await GetDData(client, modbusAddress, indicatorDiagram, (ushort)(regAddress + i), itemCount, tryReadTimes);

                    if (!isSuccess)
                    {
                        var message = "从 " + (regAddress + i).ToString() + " 个开始，读取 " + itemCount.ToString() + " 个有线功图位移数据异常!";
                        FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "Get_LYQH_WG_IndicatorDiagram", message);
                        return false;
                    }
                    Thread.Sleep(100);
                }

                var maxDis = indicatorDiagram.D.Max();
                var minDis = indicatorDiagram.D.Min();

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
                return false;
            }
            return true;
        }

        public static async Task<bool> GetLData(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                    var value = client.ByteTransform.TransInt16(read.Content, j * 2);
                    if (value != 0)
                    {
                        var L = value * 0.01;
                        indicatorDiagram.L.Add(Math.Round(L, 2));
                    }
                    else
                    {
                        indicatorDiagram.L.Add(value);
                    }
                }
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetLData(client, modbusAddress, indicatorDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetDData(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                    var value = client.ByteTransform.TransInt16(read.Content, j * 2);

                    if (value != 0)
                    {
                        indicatorDiagram.D.Add(value);
                    }
                    else
                    {
                        indicatorDiagram.D.Add(value);
                    }
                }
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDData(client, modbusAddress, indicatorDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetStroke(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                indicatorDiagram.Stroke = value * 0.001;
                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetStroke(client, modbusAddress, indicatorDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }

        public static async Task<bool> GetCount(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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
                indicatorDiagram.Count = value;
                return true;
            }
            else
            {
                tryReadTimes--;

                if (tryReadTimes > 0)
                {
                    return await GetCount(client, modbusAddress, indicatorDiagram, startAddress, regCount, tryReadTimes);
                }

                return false;
            }
        }

        public static async Task<bool> GetDateTime(ModbusRtuOverTcp client, int modbusAddress, IotDataOilWellIndicatorDiagram indicatorDiagram, ushort startAddress, ushort regCount, int tryReadTimes)
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

                indicatorDiagram.DateTime = new DateTime(2000 + year, month, date, h, m, s);

                return true;
            }
            else
            {
                tryReadTimes--;
                if (tryReadTimes > 0)
                {
                    return await GetDateTime(client, modbusAddress, indicatorDiagram, startAddress, regCount, tryReadTimes);
                }
                return false;
            }
        }


        public static async Task POST_LYQH_GTShiShiCaiJI(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var startAddress = (ushort)((slotId * 0x100) + 0x0060);

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 00, 01, 20, 08, 27, 16, 26, 0 });

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

                    //redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                    //redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                    //redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);
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

        public static async Task POST_LYQH_WG_IndicatorDiagramSetting(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var slotId = Convert.ToInt32(jo1["1"].ToString());

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                var startAddress = (ushort)0x0000;

                //写入10个寄存器
                /*功图ID：20151 10017     
                采集周期：600
                功图模式：0x10
                功图点数：200
                压缩标志：0
                MB地址：1
                槽位：2
                仪表类型：0x10
                */

                //2015110017转成16进制0x781C 2381‬，0x781C一个寄存器，0x2381一个寄存器
                // 781C  2381   遵从CDAB   变为 2381   781C

                //9089   30748

                byte[] macAddress = { 0x47, 0x92, 0x50, 0xFE, 0xFF, 0x14, 0x2E, 0x84 };

                var data = GetSettingData(2020040107, 4, 1, 1800, 0x10, 200, macAddress);

                //ushort[] data = { 0x10, 9089, 30748, 1, 2, 600, 0, 0x10, 200, 0, 38453, 65104, 5375, 33838 };

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", data);

                var flag = true;
                if (read.IsSuccess)
                {


                }
                else
                {
                    flag = false;
                    FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "POST_LYQH_WG_IndicatorDiagramSetting", "功图设置异常！");
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

        public static async Task POST_LYQH_RTUTimeSetting(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var slotId = Convert.ToInt32(jo1["1"].ToString());

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                var startAddress = (ushort)0x6000;

                var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 20, 08, 27, 16, 19, 0 });


                var flag = true;
                if (read.IsSuccess)
                {


                }
                else
                {
                    flag = false;
                    FailLog.Write(redisClient, par, indicatorDiagram, logIotModbusPoll, "POST_LYQH_RTUTimeSetting", "RTU时间设置异常！");
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


        public static ushort[] GetSettingData(int id, ushort modbusAddress, ushort slot, ushort period, ushort mode, ushort count, byte[] macAddress)
        {
            var idByte = TransIdToByteByMoveBit(id);

            var idData = GetIDData(idByte);

            var address = GetMACData(macAddress);

            ushort[] data = { 0x10, idData.Item1, idData.Item2, modbusAddress, slot, period, 0, mode, count, 0, address.Item1, address.Item2, address.Item3, address.Item4 };

            return data;
        }


        public static byte[] TransIdToByteByMoveBit(int s)
        {
            //设备编号2020010001
            byte[] binfo = new byte[4];
            binfo[0] = (byte)(s >> 8);
            binfo[1] = (byte)s;
            binfo[2] = (byte)(s >> 24);
            binfo[3] = (byte)(s >> 16);
            return binfo;
        }

        public static (ushort, ushort) GetIDData(byte[] t)
        {
            var v1 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t, 0);
            var v2 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t, 2);
            return (v1, v2);
        }

        public static (ushort, ushort, ushort, ushort) GetMACData(byte[] t)
        {
            //byte[] t = { 0x35, 0x96, 0x50, 0xFE, 0xFF, 0x14, 0x2E, 0x84 };

            var t1 = HiLowTranse(t);

            var v1 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t1, 0);
            var v2 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t1, 2);
            var v3 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t1, 4);
            var v4 = new ModbusRtuOverTcp().ByteTransform.TransUInt16(t1, 6);

            return (v1, v2, v3, v4);
        }

        public static byte[] HiLowTranse(byte[] source)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < source.Length; i = i + 2)
            {
                var s1 = source[i];
                var s2 = source[i + 1];

                list.Add(s2);
                list.Add(s1);
            }
            return list.ToArray();
        }
    }
}
