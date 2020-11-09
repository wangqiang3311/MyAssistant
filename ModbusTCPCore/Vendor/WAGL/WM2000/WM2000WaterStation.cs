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
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.WaterWell;

namespace YCIOT.ModbusPoll.Vendor.WAGL.WM2000
{
    /// <summary>
    /// 读取泵压
    /// </summary>
    public static class PumpPressure
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_XAGL_PumpPressure(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var pumpPressure = new IotDataPumpPressure();
            var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

            var modbusAddress = par.ModbusAddress;
            pumpPressure.DeviceTypeId = par.DeviceTypeId;
            ClientInfo.CurrentModbusPoolAddress = modbusAddress;

            var flag = true;
            try
            {
                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x04;
                    ClientInfo.ExpectedDataLen = 0x02;
                }
                //偏移地址根据项目待定
                var read = await client.ReadAsync($"s={modbusAddress};x=4;10", 1);
                if (read.IsSuccess)
                {
                    pumpPressure.PumpId = par.DeviceId; //泵编号
                    var value = client.ByteTransform.TransInt16(read.Content, 0);

                    //是否有变比根据项目待定
                    pumpPressure.PumpPressure = value / 100.0;//泵压
                    pumpPressure.DateTime = DateTime.Now; //采集时间
                }
                else
                {
                    flag = false;
                    pumpPressure.AlarmCode = -1;
                    pumpPressure.AlarmMsg = "数据异常";

                    pumpPressure.Mock = par.UseMockData;
                    logIotModbusPoll.DateTime = DateTime.Now;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = "读取泵压数据异常！";

                    redisClient.AddItemToList("YCIOT:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }

                if (flag == true || par.UseMockData)
                {
                    pumpPressure.Mock = par.UseMockData;
                    pumpPressure.NetworkNode = ClientInfo.ManyIpAddress;
                    //用于将读取的结果写入Redis队列
                    redisClient.AddItemToList("YCIOT:IOT_Data_PumpPressure", pumpPressure.ToJson().IndentJson());
                    redisClient.Set($"Group:PumpPressure:{par.DeviceName}-{par.DeviceId}", pumpPressure);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, pumpPressure.ToJson().IndentJson());
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

        public static void Get_XAGL_PumpPressure_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            var pumpPressure = new IotDataPumpPressure();
            pumpPressure.DeviceTypeId = par.DeviceTypeId;

            try
            {
                pumpPressure.AlarmCode = 0;
                pumpPressure.AlarmMsg = "mock数据";
                pumpPressure.Mock = par.UseMockData;

                pumpPressure.PumpId = par.DeviceId;  //泵编号
                pumpPressure.Mock = par.UseMockData;
                pumpPressure.DateTime = DateTime.Now;
                //用于将读取的结果写入Redis队列
                redisClient.AddItemToList("YCIOT:IOT_Data_PumpPressure", pumpPressure.ToJson().IndentJson());
                redisClient.Set($"Group:PumpPressure:{par.DeviceName}-{par.DeviceId}", pumpPressure);


                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, pumpPressure.ToJson().IndentJson());
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
