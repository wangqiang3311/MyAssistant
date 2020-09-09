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
using YCIOT.ServiceModel.IOT;
using Acme.Common.Utils;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
    // ReSharper disable once InconsistentNaming
    public static class WM3000KZG
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static async Task Post_XAGL_WM3000KZG_StartWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                /*
                  65 03 10 06 00 01 68 ef 
                   65 03 1a 32 00 01 2a f9
                   65 03 10 0a 00 01 a8 ec 
                   65 06 10 19 00 01 95 29
                 */
                var modbusAddress = par.ModbusAddress;

                if (!par.UseMockData)
                {
                    var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4102", 1);
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;6706", 1);

                    //（1）读4106(Holding register)寄存器为1表示正转运行，3表示停机状态。
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4106", 1);

                    //（2）写入1值到4121(Holding register)寄存器，设置油井的状态为开启。
                    //var result2 = await client.WriteAsync($"s={par.ModbusAddress};x=6;4121", (ushort)(1));


                    var startAddress = (ushort)4121;

                    var address = $"s={par.ModbusAddress};{startAddress}";

                    client.WriteOneRegister(address, (ushort)(1));


                    //（3）读取4122(Holding register)寄存器，判断状态是否写入。
                    //var result3 = await client.ReadHoldingRegisters(modbusAddress, 4102, 1);


                    //（4）读取4102(Holding register)寄存器，读取启动状态。
                    //var result14 = await client.ReadHoldingRegisters(modbusAddress, 4102, 1);

                    //（5）读取6706(Holding register)寄存器，读取告警时间长度。
                    //var result15 = await client.ReadHoldingRegisters(modbusAddress, 6706, 1);
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

        public static async Task Post_XAGL_WM3000KZG_StopWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            /*
             * 65 03 10 06 00 01 68 ef 
              65 03 1a 32 00 01 2a f9 
              65 03 10 0a 00 01 a8 ec 
              65 06 10 19 00 06 d4 eb
              */
            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                if (!par.UseMockData)
                {
                    var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4102", 1);
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;6706", 1);


                    //（1）读4106(Holding register)寄存器为1表示正转运行，3表示停机状态。
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4106", 1);

                    //写入0值到4121(Holding register)寄存器，设置油井的状态为开启。
                    //var result2 = await client.WriteAsync($"s={ par.ModbusAddress};x=6;4121", (ushort)(6));


                    var startaddress = (ushort)4121;

                    var address = $"s={par.ModbusAddress};{startaddress}";

                    client.WriteOneRegister(address, (ushort)(6));



                    //（3）读取4121(Holding register)寄存器，判断状态是否写入。
                    //var result3 = await client.ReadHoldingRegisters(modbusAddress, 4121, 1);

                    //（4）读取1283(Input status)寄存器，置位为1表示喇叭正在报警。
                    //var result4 = await client.ReadHoldingRegisters(modbusAddress, 4102, 1);  //1

                    //（5）读取1537(Holding register)寄存器，读取告警时间长度。
                    //var result5 = await client.ReadHoldingRegisters(modbusAddress, 6706, 1); //100

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

        public static async Task Post_XAGL_WM3000KZG_StrokeFrequency(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();
            try
            {
                var modbusAddress = par.ModbusAddress;

                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                //await client.WriteAsync($"s={ par.ModbusAddress};x=6;7629", (ushort)(1));

                var startAddress = (ushort)7629;

                var address = $"s={par.ModbusAddress};{startAddress}";

                /*
                   65 06 1d cd 00 01 d7 bd 
                   65 06 1d ce 00 19 27 b7
                   65 05 08 80 ff 00 87 96
                 */
                client.WriteOneRegister(address, (ushort)(1));


                var stroke = (int)(float.Parse(par.CommandParameter) * 10);

                //（1）7630(Holding register)寄存器为47，代表当前设定冲次4.7次。
                //await client.WriteAsync($"s={ par.ModbusAddress};x=6;7630", (ushort)(stroke));

                startAddress = (ushort)7630;

                address = $"s={par.ModbusAddress};{startAddress}";

                //
                client.WriteOneRegister(address, (ushort)(stroke));




                //（2）写2176（Coil status）置位此寄存器。
                await client.WriteAsync($"s={ par.ModbusAddress};x=5;2176", true);


                //（3）读7627(Holding register)寄存器为47，代表当前设定冲次4.7次。
                //var result4 = await client.ReadHoldingRegisters(modbusAddress, 7627, 1);

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

                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, -1, ex.Message);
                }
            }

        }

        public static async Task Get_XAGL_WM3000KZG_ControllerParameter(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;7596", 5);

                if (read.IsSuccess && read.Content.Length == 2 * 5)
                {
                    // 电机额定功率  7596  变比 0.1
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    var eDingGongLv = Math.Round(value * 0.1, 2);

                    // 电机额定电压  7597  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 2);
                    var eDingDianYa = value;

                    // 电机额定电流  7598  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 4);
                    var eDingDianliu = Math.Round(value * 0.01, 2);

                    // 电机额定频率  7599  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 6);
                    var eDingPingLv = Math.Round(value * 0.01, 2);

                    // 电机额定转速  7600  变比 1
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
        public static async Task Get_XAGL_WM3000KZG_ControllerStatus(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
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

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 29 * 2;
                }

                var read = await client.ReadAsync($"s={par.ModbusAddress};x=3;4096", 4124 - 4096 + 1);
                if (read.IsSuccess)
                {
                    // 耗电量  4096  变比 1  
                    var value = client.ByteTransform.TransInt16(read.Content, 0);
                    iotDataOilWellControllerState.FrequencyConversionActivePower = value;

                    // 当前冲次  4097  变比 0.1
                    value = client.ByteTransform.TransInt16(read.Content, 2);
                    iotDataOilWellControllerState.Stroke = Math.Round(value * 0.1, 2);

                    // 电流平衡度  4098  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 4);
                    iotDataOilWellControllerState.CurrentBalance = Math.Round(value * 0.01 * 100, 2);

                    // 上行最大电流  4099  变比 0.0
                    value = client.ByteTransform.TransInt16(read.Content, 6);
                    iotDataOilWellControllerState.MaxUpCurrent = Math.Round(value * 0.01, 2);

                    // 下行最大电流  4100  变比 0.01
                    value = client.ByteTransform.TransInt16(read.Content, 8);
                    iotDataOilWellControllerState.MaxDownCurrent = Math.Round(value * 0.01, 2);

                    //变频器报警代码  4101  变比 1
                    value = client.ByteTransform.TransInt16(read.Content, 10);
                    iotDataOilWellControllerState.AlarmCode = value;

                    // 起停状态  4103  变比 1--->4102
                    value = client.ByteTransform.TransInt16(read.Content, 16);
                    iotDataOilWellControllerState.StartAndStopStatus = value; //2020-04-15 由6变为8

                    //当启停状态非1和3时候，直接抛弃
                    if (value != 1 && value != 3)
                    {
                        iotDataOilWellControllerState.AlarmCode = -1;
                        iotDataOilWellControllerState.AlarmMsg = "数据异常";

                        iotDataOilWellControllerState.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取变频器启停状态异常，读取异常值为:{value}";

                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                        return;
                    }

                    // 运行频率  4105  变比 1--->4103
                    value = client.ByteTransform.TransInt16(read.Content, 14);
                    iotDataOilWellControllerState.Frequency = Math.Round(value * 0.01, 2);

                    // 运行状态  4106  变比 1--->4104
                    value = client.ByteTransform.TransInt16(read.Content, 12);
                    iotDataOilWellControllerState.RunState = value;  //2020-04-15 由8变为6

                    //变频累积运行时间4105
                    value = client.ByteTransform.TransInt16(read.Content, 18);
                    iotDataOilWellControllerState.TotalRunTime = value;

                    //变频母线电压4106
                    value = client.ByteTransform.TransInt16(read.Content, 20);
                    iotDataOilWellControllerState.BusBarVoltage = Math.Round(value * 0.1, 2);

                    //工变频切换 4122
                    value = client.ByteTransform.TransInt16(read.Content, 52);
                    iotDataOilWellControllerState.WorkAndChangeFrequencySwitch = value;

                    //就地远程切换4123
                    value = client.ByteTransform.TransInt16(read.Content, 54);
                    iotDataOilWellControllerState.LocalAndFarSwitch = value;
                }
                else
                {
                    flag = false;

                    iotDataOilWellControllerState.AlarmCode = -1;
                    iotDataOilWellControllerState.AlarmMsg = "数据异常";

                    iotDataOilWellControllerState.Mock = par.UseMockData;
                    logIotModbusPoll.DateTime = DateTime.Now;
                    logIotModbusPoll.State = -1;
                    logIotModbusPoll.Result = $"读取变频器状态数据异常！{read.Message}";

                    $"{logIotModbusPoll.Result}".Info();

                    redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                }

                if (flag)
                {
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 4;
                    }
                    //抽油机冲程7632--
                    read = await client.ReadAsync($"s={par.ModbusAddress};x=3;7632", 2);

                    if (read.IsSuccess)
                    {
                        var value = client.ByteTransform.TransInt16(read.Content, 0);
                        iotDataOilWellControllerState.MaxDisplacement = Math.Round(value * 0.1, 2);
                    }
                    else
                    {
                        flag = false;

                        iotDataOilWellControllerState.AlarmCode = -1;
                        iotDataOilWellControllerState.AlarmMsg = "数据异常";
                        iotDataOilWellControllerState.Mock = par.UseMockData;
                        logIotModbusPoll.DateTime = DateTime.Now;
                        logIotModbusPoll.State = -1;
                        logIotModbusPoll.Result = $"读取冲程数据异常！[{read.Message}]";
                        redisClient.AddItemToList("YCIOT:ERROR:Log_IOT_Modbus_Poll", logIotModbusPoll.ToJson().IndentJson());
                    }
                }

                iotDataOilWellControllerState.NetworkNode = ClientInfo.ManyIpAddress;

                //Redis中写入设备最后状态

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
    }
}
