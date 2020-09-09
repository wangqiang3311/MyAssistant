using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Net;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ServiceModel.IOT;
using Acme.Common.Utils;
using ServiceStack.Configuration;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
    // ReSharper disable once InconsistentNaming
    public static class WM1000DB
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool isDebug = new AppSettings().Get<bool>("Modbus.IsDebug");


        public static async Task Get_XAGL_WM1000DB_PowerMeter(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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
                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4117", 1);

                if (read.IsSuccess)
                {
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    if (value == 0)
                    {
                        powerMeter.NetworkNode = ClientInfo.ManyIpAddress;
                        powerMeter.AlarmCode = 3;
                        powerMeter.AlarmMsg = "停井";
                    }
                }
                else
                {

                }
                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 9 * 2;
                }
                read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4096", 9);

                if (read.IsSuccess)
                {
                    // A相电压  4097  变比 0.1
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    powerMeter.APhaseVoltage = Math.Round(value * 0.1, 1);

                    // B相电压  4098  变比 0.1
                    value = client.ByteTransform.TransInt16(read.Content, 2);
                    powerMeter.BPhaseVoltage = Math.Round(value * 0.1, 1);

                    // C相电压  4099  变比 0.1
                    value = client.ByteTransform.TransInt16(read.Content, 4);
                    powerMeter.CPhaseVoltage = Math.Round(value * 0.1, 1);

                    // A相电流  4100  变比 0.001
                    value = client.ByteTransform.TransInt16(read.Content, 6);
                    powerMeter.APhaseCurrent = Math.Round(value * 0.001, 1);

                    // B相电流   4101  变比 0.001
                    value = client.ByteTransform.TransInt16(read.Content, 8);
                    powerMeter.BPhaseCurrent = Math.Round(value * 0.001, 1);

                    // C相电流   4102  变比 0.001
                    value = client.ByteTransform.TransInt16(read.Content, 10);
                    powerMeter.CPhaseCurrent = Math.Round(value * 0.001, 1);

                    //有功电能
                    var value1 = client.ByteTransform.TransInt16(read.Content, 12);
                    var value2 = client.ByteTransform.TransInt16(read.Content, 14);
                    powerMeter.TotalActiveEnergy = Math.Round((value1 * 65535 + value2) * 0.15, 3);

                    //总功率因素  4105  变比 1
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
                    logIotModbusPoll.Result = "读取电参数据异常！";

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }

                powerMeter.NetworkNode = ClientInfo.ManyIpAddress;

                //用于将读取的结果写入Redis队列 
                if (flag == true || par.UseMockData)
                {
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

            // 
        }
    }
}
