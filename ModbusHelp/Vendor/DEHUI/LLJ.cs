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
using YCIOT.ServiceModel;
using YCIOT.ServiceModel.OilWell;

namespace YCIOT.ModbusPoll.Vendor.LLJ
{
    /// <summary>
    /// 流量计
    /// </summary>
    public class LLJ
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static async Task Get_DEHUI_LLJ(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequestDeHui>();
            try
            {
                var lLJ = new IotDataOilwellDHMeasure()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                logIotModbusPoll.State = 0;
                logIotModbusPoll.Result = "ok";

                var modbusAddress = par.ModbusAddress;
                lLJ.DeviceTypeId = par.DeviceTypeId;
                lLJ.Mock = false;
                ClientInfo.LinkId = par.LinkId;
                ClientInfo.CurrentModbusPoolAddress = modbusAddress;

                lLJ.DateTime = DateTime.Now;

                var flag = true;

                //计量自动   0地址表示线圈寄存器，读取线圈寄存器，功能码为1
                var startAddress = (ushort)(2059 - 1);
                flag = await ReadCoil(modbusAddress, startAddress, client, (value) => lLJ.JLZD = value, redisClient, par, lLJ, logIotModbusPoll, "计量自动");

                #region 批量按位读取线圈，从切井：002569 到温度保护：002576
                if (flag)
                {
                    "从切井自动批量开始读取8个线圈，读到温度保护:                ---------------".Info();
                    startAddress = (ushort)(2569 - 1);
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x01;
                        ClientInfo.ExpectedDataLen = 1;
                    }

                    var readCoil = await client.ReadCoilAsync($"s={modbusAddress};x=1;{startAddress}", 8);
                    if (readCoil.IsSuccess)
                    {
                        //False,False,False,False,True,True,False,True
                        var values = readCoil.Content;

                        lLJ.QJZD = values[0];  //切井自动
                        lLJ.WXAN = values[1];  //维修按钮

                        //[2];  //定时判断
                        //[3];  //循井测液

                        lLJ.SBGYBH = values[4];  //设备高压保护

                        lLJ.QTBH = values[5];  //气体保护

                        lLJ.SDBH = values[6];  //湿度保护

                        lLJ.WDBH = values[7];  //温度保护
                    }
                    else
                    {
                        FailLog.Write(redisClient, par, lLJ, logIotModbusPoll, "Get_LLJ", "读取切井自动批量异常！");
                        flag = false;
                    }
                    "切井自动批量结束:                ---------------".Info();
                }
                #endregion

                //排风机
                if (flag)
                {
                    startAddress = (ushort)(1281 - 1);
                    flag = await ReadCoil(modbusAddress, startAddress, client, (value) => lLJ.PFJ = value, redisClient, par, lLJ, logIotModbusPoll, "排风机");
                }

                //加热器
                if (flag)
                {
                    startAddress = (ushort)(1282 - 1);
                    flag = await ReadCoil(modbusAddress, startAddress, client, (value) => lLJ.JRQ = value, redisClient, par, lLJ, logIotModbusPoll, "加热器");
                }

                #region from:进液压力:404097  to:分钟含水:404121
                if (flag)
                {
                    startAddress = (ushort)(4097 - 1);
                    ("进液压力开始:                ---------------").Info();

                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 26 * 2;
                    }
                    var read = await client.ReadAsync($"s={modbusAddress};x=3;{startAddress}", 26);

                    if (read.IsSuccess)
                    {
                        var souce = Tools.CDAB(read.Content, 0);
                        var value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.JYYL = value;

                        souce = Tools.CDAB(read.Content, 4);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.CYYL = value;

                        souce = Tools.CDAB(read.Content, 8);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.HJWD = value;

                        souce = Tools.CDAB(read.Content, 12);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.HJSD = value;

                        souce = Tools.CDAB(read.Content, 16);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.HJND = value;

                        souce = Tools.CDAB(read.Content, 20);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.HSWZ = value;

                        souce = Tools.CDAB(read.Content, 24);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SFFK = value;

                        souce = Tools.CDAB(read.Content, 28);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.XFFK = value;

                        souce = Tools.CDAB(read.Content, 32);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.WTF1FK = value;

                        souce = Tools.CDAB(read.Content, 36);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.WTF2FK = value;

                        souce = Tools.CDAB(read.Content, 40);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.WTF3FK = value;

                        souce = Tools.CDAB(read.Content, 44);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.PGWD = value;

                        souce = Tools.CDAB(read.Content, 48);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.FZHS = value;
                    }
                    else
                    {
                        FailLog.Write(redisClient, par, lLJ, logIotModbusPoll, "Get_LLJ", "读取进液压力异常！");
                        flag = false;
                    }
                    "进液压力结束:                ---------------".Info();
                }

                #endregion

                #region  from:累计产液：404505  to:右限液位：404553

                if (flag)
                {
                    startAddress = (ushort)(4505 - 1);
                    "累计产液开始:                ---------------".Info();

                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 50 * 2;
                    }
                    var read = await client.ReadAsync($"s={modbusAddress};x=3;{startAddress}", 50);

                    if (read.IsSuccess)
                    {
                        var souce = Tools.CDAB(read.Content, 0);
                        var value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.LJCY = value;

                        souce = Tools.CDAB(read.Content, 4);

                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.DCRJ = value;

                        souce = Tools.CDAB(read.Content, 8);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SSCY = value;

                        souce = Tools.CDAB(read.Content, 12);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SSPJHS = value;

                        souce = Tools.CDAB(read.Content, 16);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.ZRHS = value;

                        souce = Tools.CDAB(read.Content, 20);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.YLGX = value;

                        souce = Tools.CDAB(read.Content, 24);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.ZRCY = value;

                        souce = Tools.CDAB(read.Content, 28);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.YLDX = value;

                        var valueInt = client.ByteTransform.TransInt16(read.Content, 32);
                        lLJ.QYBH = valueInt;

                        valueInt = client.ByteTransform.TransInt16(read.Content, 34);
                        lLJ.JLQLX = valueInt;

                        souce = Tools.CDAB(read.Content, 36);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SFKZ = value;

                        souce = Tools.CDAB(read.Content, 40);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.XFKZ = value;

                        valueInt = client.ByteTransform.TransInt16(read.Content, 44);
                        lLJ.SSYJH = valueInt;
                        lLJ.WellId = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 46);
                        lLJ.ZRYJH = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 48);
                        lLJ.ZRJH = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 50);
                        lLJ.SSJH = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 52);
                        lLJ.ZCJS = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 54);
                        lLJ.SZXS = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 56);
                        lLJ.SZFZ = valueInt;

                        //巡井手切小时、分钟没有读

                        value = client.ByteTransform.TransInt16(read.Content, 62);
                        lLJ.YCYSSJ = valueInt;

                        //加热器次数、排风机次数没有读

                        value = client.ByteTransform.TransInt16(read.Content, 68);
                        lLJ.ZTMFM = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 70);
                        lLJ.ZTMJSY = valueInt;

                        value = client.ByteTransform.TransInt16(read.Content, 72);
                        lLJ.SZZDQJZQ = valueInt;

                        //4542预留 
                        souce = Tools.CDAB(read.Content, 76);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.WDDX = value;

                        souce = Tools.CDAB(read.Content, 80);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.WDGX = value;

                        souce = Tools.CDAB(read.Content, 84);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.NDDX = value;

                        souce = Tools.CDAB(read.Content, 88);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.NDGX = value;

                        souce = Tools.CDAB(read.Content, 92);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.ZXYW = value;

                        souce = Tools.CDAB(read.Content, 96);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.YXYW = value;

                    }
                    else
                    {
                        FailLog.Write(redisClient, par, lLJ, logIotModbusPoll, "Get_LLJ", "读取累计产液异常！");
                        flag = false;
                    }
                    "累计产液结束:                ---------------".Info();
                }
                #endregion

                #region 湿度高限：404567 to:湿度低限：404569
                if (flag)
                {
                    startAddress = (ushort)(4567 - 1);
                    ("湿度高限开始:                ---------------").Info();
                    lock (ClientInfo.locker)
                    {
                        ClientInfo.RequestTime = DateTime.Now;
                        ClientInfo.ExpectedType = 0x03;
                        ClientInfo.ExpectedDataLen = 4 * 2;
                    }
                    var read = await client.ReadAsync($"s={modbusAddress};x=3;{startAddress}", 4);

                    if (read.IsSuccess)
                    {
                        var souce = Tools.CDAB(read.Content, 0);
                        var value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SDGX = value;

                        souce = Tools.CDAB(read.Content, 4);
                        value = client.ByteTransform.TransSingle(souce, 0);
                        lLJ.SDDX = value;
                    }
                    else
                    {
                        flag = false;
                        FailLog.Write(redisClient, par, lLJ, logIotModbusPoll, "Get_LLJ", "读取湿度高限异常！");
                    }
                    "湿度高限结束:                ---------------".Info();
                }
                #endregion

                lLJ.NetworkNode = ClientInfo.ManyIpAddress;

                if (flag == true || par.UseMockData)
                {
                    lLJ.Mock = par.UseMockData;

                    $"CommandParameter;{par.CommandParameter}".Info();
                    //根据获取的井映射到实际的井
                    var wellIdDic = JsonConvert.DeserializeObject<Dictionary<int, int>>(par.CommandParameter);
                    if (wellIdDic.ContainsKey(lLJ.SSJH))
                    {
                        lLJ.WellId = wellIdDic[lLJ.SSJH];
                        lLJ.StationId = par.StationId;
                        lLJ.StationName = par.StationName;
                        lLJ.WellFieldId = par.DeviceId;
                        lLJ.WellFieldName = par.DeviceName;
                        redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_lLJ", lLJ.ToJson().IndentJson());
                        redisClient.Set($"Group:OilWell:{lLJ.WellId}:lLJ", lLJ);
                        redisClient.Set($"Single:OilWell:lLJ:{lLJ.WellId}", lLJ);
                    }
                }

                //用于通过ServerEvent给调用着返回消息
                if (!par.UserName.IsNullOrEmpty())
                {
                    ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, flag ? 0 : -2, lLJ.ToJson().IndentJson());
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


        public static async Task<bool> ReadCoil(int modbusAddress, ushort startAddress, ModbusRtuOverTcp client
            , Action<bool> ReadSuccess, RedisClient redisClient, ControlRequest par
            , IotDataOilwellDHMeasure lLJ, LogIotModbusPoll logIotModbusPoll, string message)
        {
            $"{message}开始:                ---------------".Info();

            lock (ClientInfo.locker)
            {
                ClientInfo.RequestTime = DateTime.Now;
                ClientInfo.ExpectedType = 0x01;
                ClientInfo.ExpectedDataLen = 1;
            }
            var readCoil = await client.ReadCoilAsync($"s={modbusAddress};x=1;{startAddress}", 1);
            if (readCoil.IsSuccess)
            {
                ReadSuccess?.Invoke(readCoil.Content[0]);
                return true;
            }
            else
            {
                FailLog.Write(redisClient, par, lLJ, logIotModbusPoll, "Get_LLJ", $"读取{message}异常！");
                return false;
            }
        }
    }
}
