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
using YCIOT.ServiceModel.OilWell;

namespace YCIOT.ModbusPoll.Vendor.WAGL.WM2000
{
    public static class WM2000KZG
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Post_XAGL_WM2000KZG_StartWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var modbusAddress = par.ModbusAddress;

                if (!par.UseMockData)
                {
                    var startAddress = (ushort)4131;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    client.WriteOneRegister(address, (ushort)(1));
                }

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

        public static void Post_XAGL_WM2000KZG_StopWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                if (!par.UseMockData)
                {
                    var startaddress = (ushort)4131;

                    var address = $"s={par.ModbusAddress};{startaddress}";

                    client.WriteOneRegister(address, (ushort)(6));

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

        /// <summary>
        /// 设置冲次
        /// </summary>
        /// <param name="client"></param>
        /// <param name="redisClient"></param>
        /// <param name="messageString"></param>
        /// <returns></returns>
        public static async Task Post_XAGL_WM2000KZG_StrokeFrequency(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                var stroke = (int)(float.Parse(par.CommandParameter) * 10);

                //4132(Holding register)寄存器为47，代表当前设定冲次4.7次。

                var startAddress = (ushort)4132;

                var address = $"s={par.ModbusAddress};{startAddress}";

                await client.WriteOneRegisterAsync(address, (ushort)(stroke));


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

                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                }
            }

        }
        /// <summary>
        /// 读取电机基本监视参数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="redisClient"></param>
        /// <param name="messageString"></param>
        /// <returns></returns>
        public static async Task Get_XAGL_WM2000KZG_ControllerParameter(ModbusRtuOverTcp client, RedisClient redisClient, string messageString, IotDataOilWellControllerState controllerState = null)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 10;
                }

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4116", 5);

                if (read.IsSuccess)
                {
                    // 电机额定功率  4116  变比 0.1
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    var eDingGongLv = Math.Round(value * 0.1, 2);

                    // 电机额定电压  4117  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 2);
                    var eDingDianYa = value;

                    // 电机额定电流  4118  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 4);
                    var eDingDianliu = Math.Round(value * 0.01, 2);

                    // 电机额定频率  4119  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 6);
                    var eDingPingLv = Math.Round(value * 0.01, 2);

                    // 电机额定转速  4120  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 8);
                    var eDingZhuanSu = value;
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
        /// 读取控制器数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="redisClient"></param>
        /// <param name="messageString"></param>
        /// <returns></returns>
        public static async Task Get_XAGL_WM2000KZG_ControllerStatus(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var iotDataOilWellControllerState = new IotDataOilWellControllerState()
                {
                    NetworkNode = ClientInfo.ManyIpAddress,
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };
                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                logIotModbusPoll.State = 0;
                logIotModbusPoll.Result = "ok";

                var modbusAddress = par.ModbusAddress;
                iotDataOilWellControllerState.DateTime = DateTime.Now;
                iotDataOilWellControllerState.DeviceTypeId = par.DeviceTypeId;
                iotDataOilWellControllerState.Mock = false;

                var flag = true;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                iotDataOilWellControllerState.DateTime = DateTime.Now;
                iotDataOilWellControllerState.WellId = par.DeviceId;


                #region  变频器基本监视参数

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 9 * 2;
                }

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4096", 4104 - 4096 + 1);
                if (read.IsSuccess)
                {
                    // 耗电量  4096  变比 1  
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    iotDataOilWellControllerState.FrequencyConversionActivePower = value;

                    //电流平衡度  4097 变比  0.01
                    value = client.ByteTransform.TransInt16(read.Content, 2);
                    iotDataOilWellControllerState.CurrentBalance = Math.Round(value * 0.01, 2);

                    // 上行最大电流  4098  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 4);
                    iotDataOilWellControllerState.MaxUpCurrent = Math.Round(value * 0.01, 2);

                    // 下行最大电流  4099  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 6);
                    iotDataOilWellControllerState.MaxDownCurrent = Math.Round(value * 0.01, 2);

                    //变频器报警代码  4100  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 8);
                    iotDataOilWellControllerState.AlarmCode = value;

                    // 起停状态  4101  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 10);
                    iotDataOilWellControllerState.StartAndStopStatus = value;

                    // 运行频率  4102  变比0.01
                    value = client.ByteTransform.TransInt16(read.Content, 12);
                    iotDataOilWellControllerState.Frequency = Math.Round(value * 0.01, 2);

                    // 运行状态  4103  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 14);
                    iotDataOilWellControllerState.RunState = value;

                    //变频功率因素角度
                    value = client.ByteTransform.TransInt16(read.Content, 16);
                    iotDataOilWellControllerState.PowerFactorAngle = value;
                }
                else
                {
                    flag = false;

                    iotDataOilWellControllerState.AlarmCode = -1;
                    iotDataOilWellControllerState.AlarmMsg = "数据异常";

                    iotDataOilWellControllerState.Mock = par.UseMockData;
                    logIotModbusPoll.DateTime = DateTime.Now;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取变频器基本监视参数异常！{read.Message}";

                    $"{logIotModbusPoll.Result}".Info();

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }

                #endregion


                #region   工变频切换、就地远程切换

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 4;
                    }

                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4128", 3);

                    if (read.IsSuccess)
                    {
                        //工变频切换 4128    0工；1变
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        iotDataOilWellControllerState.WorkAndChangeFrequencySwitch = value;

                        //就地远程切换4129   0就；1远
                        value = client.ByteTransform.TransInt16(read.Content, 2);
                        iotDataOilWellControllerState.LocalAndFarSwitch = value;

                        //点数采集4130
                        value = client.ByteTransform.TransInt16(read.Content, 4);
                        iotDataOilWellControllerState.CountFetchAddress = value;
                    }
                    else
                    {
                        flag = false;

                        iotDataOilWellControllerState.AlarmCode = -1;
                        iotDataOilWellControllerState.AlarmMsg = "数据异常";
                        iotDataOilWellControllerState.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取工变频切换和就地远程切换异常！[{read.Message}]";
                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                #endregion


                #region  双字节监视参数

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 8 * 2;
                    }

                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4133", 8);

                    if (read.IsSuccess)
                    {
                        //变频累积运行时间
                        var value0 = client.ByteTransform.TransInt16(read.Content, 0);
                        var value1 = client.ByteTransform.TransInt16(read.Content, 2);

                        iotDataOilWellControllerState.TotalRunTime = value1 * 10000 + value0;

                        //变频累积上电时间
                        value0 = client.ByteTransform.TransInt16(read.Content, 4);
                        value1 = client.ByteTransform.TransInt16(read.Content, 6);

                        iotDataOilWellControllerState.TotalPowerupTime = value1 * 10000 + value0;

                        //变频当前上电时间
                        value0 = client.ByteTransform.TransInt16(read.Content, 8);
                        value1 = client.ByteTransform.TransInt16(read.Content, 10);
                        iotDataOilWellControllerState.CurrentPowerupTime = value1 * 10000 + value0;

                        //变频当前运行时间
                        value0 = client.ByteTransform.TransInt16(read.Content, 12);
                        value1 = client.ByteTransform.TransInt16(read.Content, 14);
                        iotDataOilWellControllerState.CurrentRunTime = value1 * 10000 + value0;
                    }
                    else
                    {
                        flag = false;

                        iotDataOilWellControllerState.AlarmCode = -1;
                        iotDataOilWellControllerState.AlarmMsg = "数据异常";
                        iotDataOilWellControllerState.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取变频累积运行时间异常！[{read.Message}]";
                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                #endregion


                #region 其它参数(井口压力）

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 3 * 2;
                    }

                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4166", 3);

                    if (read.IsSuccess)
                    {
                        //井口压力1   4166
                        var value = client.ByteTransform.TransInt16(read.Content, 0);

                        //井口压力2   4167
                        value = client.ByteTransform.TransInt16(read.Content, 2);

                        //井口压力3   4168
                        value = client.ByteTransform.TransInt16(read.Content, 4);
                    }
                    else
                    {
                        flag = false;

                        iotDataOilWellControllerState.AlarmCode = -1;
                        iotDataOilWellControllerState.AlarmMsg = "数据异常";
                        iotDataOilWellControllerState.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取井口压力数据异常！[{read.Message}]";
                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                #endregion


                iotDataOilWellControllerState.NetworkNode = ClientInfo.ManyIpAddress;

                //写入设备最后状态

                if (flag == true || par.UseMockData)
                {
                    iotDataOilWellControllerState.Mock = par.UseMockData;

                    redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_ControllerState", iotDataOilWellControllerState.ToJson().IndentJson());
                    redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:ControllerState", iotDataOilWellControllerState);
                    redisClient.Set($"Single:OilWell:ControllerState:{par.DeviceName}-{par.DeviceId}", iotDataOilWellControllerState);
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, iotDataOilWellControllerState.ToJson().IndentJson());
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

        public static void Get_XAGL_WM2000KZG_ControllerStatus_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var iotDataOilWellControllerState = new IotDataOilWellControllerState()
                {
                    NetworkNode = ClientInfo.ManyIpAddress,
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                iotDataOilWellControllerState.WellId = par.DeviceId;
                iotDataOilWellControllerState.DeviceTypeId = par.DeviceTypeId;
                iotDataOilWellControllerState.DateTime = DateTime.Now;
                iotDataOilWellControllerState.Mock = par.UseMockData;

                redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_ControllerState", iotDataOilWellControllerState.ToJson().IndentJson());
                redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:ControllerState", iotDataOilWellControllerState);
                redisClient.Set($"Single:OilWell:ControllerState:{par.DeviceName}-{par.DeviceId}", iotDataOilWellControllerState);

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

        public static void Get_XAGL_WM2000KZG_ControllerParameter_Mock(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            Get_XAGL_WM2000KZG_ControllerStatus_Mock(client, redisClient, messageString);
        }
    }
}
