using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Net;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ServiceModel;
using Acme.Common.Utils;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.XAAS
{
    // ReSharper disable once InconsistentNaming
    public static class KZG
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Post_XAAS_WII_StartWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var modbusAddress = par.ModbusAddress;

                if (!par.UseMockData)
                {
                    //写入81值到40491(Holding register)寄存器，设置油井的状态为开启。

                    var startAddress = (ushort)40491;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    client.WriteOneRegister(address, (ushort)(81));
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                Logger.Error(ex.Source);

                if (par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                }
            }
        }

        public static void Post_XAAS_WII_StopWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                if (!par.UseMockData)
                {
                    //写入82值到40491(Holding register)寄存器，设置油井的状态为停止。

                    var startAddress = (ushort)40491;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    client.WriteOneRegister(address, (ushort)(82));

                    //用于通过ServerEvent给调用着返回消息
                    if (!par.UserName.IsNullOrEmpty())
                    {
                        ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, 0, "ok");
                    }
                }
                else
                {
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
