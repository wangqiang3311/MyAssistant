using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Net;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ServiceModel;
using HslCommunication.ModBus;
using Acme.Common.Utils;
using YCIOT.ServiceModel.OilWell;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL.WM3000
{
    // ReSharper disable once InconsistentNaming
    public static class WM3000DB
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 读取电表数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="redisClient"></param>
        /// <param name="messageString"></param>
        /// <returns></returns>
        public static async Task Get_XAGL_WM3000DB_PowerMeter(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;
                ClientInfo.LinkId = par.LinkId;

                powerMeter.DateTime = DateTime.Now;
                powerMeter.WellId = par.DeviceId;

                powerMeter.DeviceTypeId = par.DeviceTypeId;
                powerMeter.Mock = false;

                var flag = true;

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
                    if (value == 3)
                    {
                        powerMeter.NetworkNode = ClientInfo.ManyIpAddress;

                        powerMeter.AlarmCode = 3;
                        powerMeter.AlarmMsg = "停井";
                    }
                }
                else
                {
                    flag = false;
                    powerMeter.AlarmCode = -1;
                    powerMeter.AlarmMsg = "数据异常";

                    powerMeter.Mock = par.UseMockData;
                    logIotModbusPoll.DateTime = DateTime.Now;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取电参数据异常！[{read.Message}]";
                    $"DB:{par.DeviceName}-{par.DeviceId}{logIotModbusPoll.Result}".Info();

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }


                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 18;
                    }

                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;38668", 9);
                    if (read.IsSuccess)
                    {
                        // A相电压  38668  变比 0.1
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        powerMeter.APhaseVoltage = Math.Round(value * 0.1, 1);

                        // B相电压  38669  变比 0.1
                        value = client.ByteTransform.TransInt16(read.Content, 2);
                        powerMeter.BPhaseVoltage = Math.Round(value * 0.1, 1);

                        // C相电压  38670  变比 0.1
                        value = client.ByteTransform.TransInt16(read.Content, 4);
                        powerMeter.CPhaseVoltage = Math.Round(value * 0.1, 1);

                        // A相电流  38671  变比 0.015
                        value = client.ByteTransform.TransInt16(read.Content, 6);
                        powerMeter.APhaseCurrent = Math.Round(value * 0.015, 1);

                        // B相电流   38672  变比 0.015
                        value = client.ByteTransform.TransInt16(read.Content, 8);
                        powerMeter.BPhaseCurrent = Math.Round(value * 0.015, 1);

                        // C相电流   38673  变比 0.015
                        value = client.ByteTransform.TransInt16(read.Content, 10);
                        powerMeter.CPhaseCurrent = Math.Round(value * 0.015, 1);

                        // 总功率因素  38676  变比 0.001
                        value = client.ByteTransform.TransInt16(read.Content, 16);
                        powerMeter.TotalPowerFactor = Math.Round(value * 0.001, 1);
                    }
                    else
                    {
                        flag = false;
                        powerMeter.AlarmCode = -1;
                        powerMeter.AlarmMsg = "数据异常";

                        powerMeter.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取电参数据异常！[{read.Message}]";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 8;
                    }
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;6896", 4);
                    if (read.IsSuccess)
                    {
                        ////这是总有功电能 
                        //var value1 = client.ByteTransform.TransInt16(read.Content, 0);
                        //var value2 = client.ByteTransform.TransInt16(read.Content, 2);
                        //powerMeter.TotalActivePower = Math.Round(((value2 * 65536 + value1) * 0.01) * 15, 2);

                        ////这是总无功电能 
                        //value1 = client.ByteTransform.TransInt16(read.Content, 0);
                        //value2 = client.ByteTransform.TransInt16(read.Content, 2);
                        //powerMeter.TotalReactivePower = Math.Round(((value2 * 65536 + value1) * 0.01) * 15, 2);
                        var value0 = client.ByteTransform.TransInt16(read.Content, 0);
                        var value1 = client.ByteTransform.TransInt16(read.Content, 2);
                        var value2 = client.ByteTransform.TransInt16(read.Content, 4);
                        var value3 = client.ByteTransform.TransInt16(read.Content, 6);
                        //这是总有功电能 
                        powerMeter.TotalActivePower = Math.Round(((value1 * 65536 + value0) * 0.01) * 15, 2);
                        //这是总无功电能 
                        powerMeter.TotalReactivePower = Math.Round(((value3 * 65536 + value2) * 0.01) * 15, 2);
                    }
                    else
                    {
                        flag = false;

                        powerMeter.AlarmCode = -1;
                        powerMeter.AlarmMsg = "数据异常";

                        powerMeter.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取总有功电能数据异常！[{read.Message}]";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }
                powerMeter.NetworkNode = ClientInfo.ManyIpAddress;

                //用于将读取的结果写入Redis队列 
                if (flag == true || par.UseMockData)
                {
                    $"DB:{par.DeviceName}-{par.DeviceId}已获取到数据".Info();
                    powerMeter.Mock = par.UseMockData;
                    redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_PowerMeter", powerMeter.ToJson().IndentJson());
                    redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:PowerMeter", powerMeter);
                    redisClient.Set($"Single:OilWell:PowerMeter:{par.DeviceName}-{par.DeviceId}", powerMeter);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, powerMeter.ToJson().IndentJson());
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
