using Acme.Common.Utils;
using HslCommunication.ModBus;
using NLog;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using YCIOT.ModbusPoll.RtuOverTcp.Bridge;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.Utils;
using YCIOT.ModbusPoll.Vendor.BJBC;
using YCIOT.ModbusPoll.Vendor.LLJ;
using YCIOT.ModbusPoll.Vendor.LYQH;
using YCIOT.ModbusPoll.Vendor.WAGL;
using YCIOT.ModbusPoll.Vendor.WAGL.WM3000;
using YCIOT.ModbusPoll.Vendor.ZKAW;


//dotnet publish --runtime alpine.3.7-x64
//dotnet publish -f netcoreapp3.1 -c Release

namespace YCIOT.ModbusPoll.RtuOverTcp
{
    //https://github.com/BlackyPanther/Modbus/blob/master/src/ConsoleDemo/Program.cs
    internal class Core
    {
       public static void ConnectServer()
        {
          //初始化
            Tools.WriteStartLog();
            ClientInfo.RequestTime = DateTime.Now;
            ClientInfo.LinkId = -1;

            ClientInfo.cache = MemoryCache.Default;

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10)
            };


            ClientInfo.cache.Set("one", "", policy);
            ClientInfo.cache.Set("many", "", policy);

            ServiceStackHelper.Patch();
            #region Hsl注册
            if (!HslCommunication.Authorization.SetAuthorizationCode("80b4f667-3c33-4f32-9bfd-aeb10cf98096"))
            {
                Console.WriteLine("Authorization failed! The current program can only be used for 8 hours!");
                return;
            }
            #endregion
            //Console.CancelKeyPress += Console_CancelKeyPress;

            //一个大坑，没有这个前面的appSettings出不来
            var assembliesWithServices = new Assembly[1];
            assembliesWithServices[0] = typeof(AppHost).Assembly;
            var appHost = new AppHost("AppHost", assembliesWithServices);

            IAppSettings appSettings = new AppSettings();

            var localIp = appSettings.Get<string>("Modbus.LocalIP");
            var tcpPortMany = appSettings.Get<int>("Modbus.TcpPort.Many");
            var tcpPortOne = appSettings.Get<int>("Modbus.TcpPort.One");

            var modbusTimeout = appSettings.Get<int>("Modbus.Timeout", 3000);
            var groupName = appSettings.Get<string>("Modbus.GroupName");
            var jobListRedisConfig = appSettings.Get<RedisEndpoint>("Redis.ModbusJobList");
            var jobListTimeOut = appSettings.Get<int>("JobList.Timeout");
            useLinkId = appSettings.Get<bool>("Modbus.UserLinkId");
            var isDebug = appSettings.Get<bool>("Modbus.IsDebug");
            var app = appSettings.Get<string>("AppCMD");
            var isUseStrictCheckMode = appSettings.Get<string>("Modbus.IsUseStrictCheckMode");
            var appRunTime = appSettings.Get<string>("AppRunTime");


            jobListRedisConfig = Tools.GetRedisEndpoint(jobListRedisConfig);

            JsConfig.DateHandler = DateHandler.ISO8601DateTime;
            JsConfig.IncludeNullValues = true;

            Console.WriteLine("GroupName          : " + groupName);
            Console.WriteLine("Modbus.LocalIP     : " + localIp);
            Console.WriteLine("Modbus.TcpPort     : " + $"{tcpPortOne}<->{tcpPortMany}");
            Console.WriteLine("Modbus.Timeout     : " + modbusTimeout + " 毫秒");
            Console.WriteLine("Redis.Host         : " + jobListRedisConfig.Host);
            Console.WriteLine("JobList.Timeout    : " + jobListTimeOut + " 秒");
            Console.WriteLine("Modbus.UserLinkId  : " + useLinkId);
            Console.WriteLine("Modbus.IsDebug     : " + isDebug);
            Console.WriteLine("Modbus.IsUseStrictCheckMode: " + isUseStrictCheckMode);

            "程序开始执行".Info();
            redisClient = new RedisClient(jobListRedisConfig);
            var bridge = new Bridge.TcpBridge(localIp, tcpPortOne, tcpPortMany);
            var time = appRunTime.Split(',');
            var hour = int.Parse(time[0]);
            var minute = int.Parse(time[1]);
            var second = int.Parse(time[2]);

            long endTime = DateTime.Now.Add(new TimeSpan(hour, minute, second)).Ticks;

            try
            {
                client = new ModbusRtuOverTcp(localIp, tcpPortOne)
                {
                    ReceiveTimeOut = modbusTimeout
                };
                //断开长连接，转为短连接
                client.ConnectClose();
                //client.ConnectServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Task.Run[0-0]:" + ex.Message);
                Console.WriteLine("Task.Run[0-0]:" + ex.StackTrace);
            }


        }
        private static bool useLinkId;
        private static ModbusRtuOverTcp client = null;
        private static RedisClient redisClient = null;
        private const string QueudId = "YCIOT";
        public static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static async void DoWork(string commandType, int linkId,int slotId,int modbusAddress=1)
        {
            try
            {
                var messageString = "";
                ControlRequest job = null;
                messageString = DebugHelper.MockRequest(commandType, 1, modbusAddress, "Device", slotId,linkId);
                job = messageString.FromJson<ControlRequest>();

                if (job != null)
                {
                    if (useLinkId)
                    {
                        lock (ClientInfo.locker)
                        {
                            ClientInfo.RequestTime = DateTime.Now;
                            ClientInfo.LinkId = job.LinkId;
                        }
                    }
                    $"{ job.CommandType}:{job.DeviceName}:{job.DeviceId}:{job.GroupName}:{job.ModbusAddress}".Info();

                    switch (job.CommandType)
                    {
                        #region //心跳包

                        case "Heartbeat":
                            //try
                            //{
                            //    //await client.ReadHoldingRegisters(255, 1, 1);
                            //}
                            //catch
                            //{
                            //    // ignored
                            //}
                            break;

                        #endregion

                        #region  //浙江精华
                        case "Get_ZJJH_WTPM_TrunkPressure": //读取配水间干压
                            await Vendor.ZJJH.Box.Get_ZJJH_WTPM_TrunkPressure(client, redisClient, messageString);
                            break;

                        case "Get_ZJJH_WII_WaterInjectingInstrument": //读取注水仪数据
                            await Vendor.ZJJH.Box.Get_ZJJH_WII_WaterInjectingInstrument(client, redisClient, messageString);
                            break;

                        case "Post_ZJJH_WII_InjectionAllocation": //下发配注量
                            await Vendor.ZJJH.Box.Post_ZJJH_WII_InjectionAllocation(client, redisClient, messageString);
                            break;

                        case "Post_ZJJH_WII_TubePressure": //下发管压设定
                            await Vendor.ZJJH.Box.Post_ZJJH_WII_TubePressure(client, redisClient, messageString);
                            break;
                        #endregion

                        #region  //北京必创
                        case "Get_BJBC_WTPM_TrunkPressure": //读取配水间干压
                            await Box.Get_BJBC_WTPM_TrunkPressure(client, redisClient, messageString);
                            break;

                        case "Get_BJBC_WII_WaterInjectingInstrument": //读取注水仪数据
                            await Box.Get_BJBC_WII_WaterInjectingInstrument(client, redisClient, messageString);
                            break;

                        case "Post_BJBC_WII_InjectionAllocation": //下发配注量
                            await Box.Post_BJBC_WII_InjectionAllocation(client, redisClient, messageString);
                            break;
                        #endregion

                        #region  //西安国仪
                        case "Get_XAGY_WTPM_TrunkPressure": //读取配水间干压
                            await Vendor.XAGY.Box.Get_XAGY_WTPM_TrunkPressure(client, redisClient, messageString);
                            break;

                        case "Get_XAGY_WII_WaterInjectingInstrument": //读取注水仪数据
                            await Vendor.XAGY.Box.Get_XAGY_WII_WaterInjectingInstrument(client, redisClient, messageString);
                            break;

                        case "Post_XAGY_WII_InjectionAllocation": //下发配注量
                            await Vendor.XAGY.Box.Post_XAGY_WII_InjectionAllocation(client, redisClient, messageString);
                            break;

                        case "Post_XAGY_WII_TubePressure": //下发管压设定
                            await Vendor.XAGY.Box.Post_XAGY_WII_TubePressure(client, redisClient, messageString);
                            break;
                        #endregion

                        #region  //中科奥维
                        case "Get_ZKAW_IndicatorDiagram":  // 读取无线功图
                            await IndicatorDiagram.Get_ZKAW_IndicatorDiagram(client, redisClient, messageString);
                            break;
                        #endregion

                        #region //西安贵隆
                        case "Get_XAGL_WM1000YXGT_IndicatorDiagram": // 读取有线功图
                            await WM1000YXGT.Get_XAGL_WM1000YXGT_IndicatorDiagram(client, redisClient,
                                   messageString);
                            break;

                        case "Get_XAGL_WM1000DLT_CurrentDiagram": // 读取电流图
                            await WM1000DLT.Get_XAGL_WM1000DLT_CurrentDiagram(client, redisClient,
                                   messageString);
                            break;

                        case "Get_XAGL_WM1000DB_PowerMeter": // 读电参
                            await WM1000DB.Get_XAGL_WM1000DB_PowerMeter(client, redisClient,
                                   messageString);
                            break;

                        case "Post_XAGL_WM1000KZG_StartWell": // 开井操作
                            await WM1000KZG.Post_XAGL_WM1000KZG_StartWell(client, redisClient,
                                   messageString);
                            break;

                        case "Post_XAGL_WM1000KZG_StopWell": // 关井操作
                            await WM1000KZG.Post_XAGL_WM1000KZG_StopWell(client, redisClient,
                                   messageString);
                            break;

                        case "Get_XAGL_WM1000KZG_ControllerStatus": // 读变频器运行参数
                            await WM1000KZG.Get_XAGL_WM1000KZG_ControllerStatus(client, redisClient,
                                   messageString);
                            break;


                        case "Get_XAGL_WM3000WXGT_IndicatorDiagram": // 读取无线功图
                            await WM3000WXGT.Get_XAGL_WM3000WXGT_IndicatorDiagram(client, redisClient,
                                   messageString);
                            break;

                        case "Get_XAGL_WM3000YXGT_IndicatorDiagram": // 读取有线功图
                            await WM3000YXGT.Get_XAGL_WM3000YXGT_IndicatorDiagram(client, redisClient, messageString);
                            break;

                        case "Get_XAGL_WM3000GLT_PowerDiagram": // 读取功率图
                            await WM3000GLT.Get_XAGL_WM3000GLT_PowerDiagram(client, redisClient, messageString);
                            break;

                        case "Get_XAGL_WM3000DLT_CurrentDiagram": // 读取电流图
                            await WM3000DLT.Get_XAGL_WM3000DLT_CurrentDiagram(client, redisClient, messageString);
                            break;

                        case "Get_XAGL_WM3000DB_PowerMeter": // 读电参
                            await WM3000DB.Get_XAGL_WM3000DB_PowerMeter(client, redisClient, messageString);
                            break;

                        case "Post_XAGL_WM3000KZG_StartWell": // 开井操作
                            await WM3000KZG.Post_XAGL_WM3000KZG_StartWell(client, redisClient, messageString);
                            break;

                        case "Post_XAGL_WM3000KZG_StopWell": // 关井操作
                            await WM3000KZG.Post_XAGL_WM3000KZG_StopWell(client, redisClient, messageString);
                            break;

                        case "Post_XAGL_WM3000KZG_StrokeFrequency": // 下发冲次操作
                            await WM3000KZG.Post_XAGL_WM3000KZG_StrokeFrequency(client, redisClient, messageString);
                            break;

                        case "Get_XAGL_WM3000KZG_ControllerParameter": // 读电机参数
                            await WM3000KZG.Get_XAGL_WM3000KZG_ControllerParameter(client, redisClient, messageString);
                            break;

                        case "Get_XAGL_WM3000KZG_ControllerStatus": // 读变频器运行参数
                            await WM3000KZG.Get_XAGL_WM3000KZG_ControllerStatus(client, redisClient, messageString);
                            break;

                        #endregion

                        #region  //西安安森
                        case "Get_XAAS_WTPM_TrunkPressure": //读取配水间干压
                            if (useLinkId)
                            {
                                var mclient = TcpBridge.ManyClients.Values.FirstOrDefault(c => c.LinkId.HasValue && c.LinkId == job.LinkId);
                                if (mclient != null)
                                {
                                    await Vendor.XAAS.Box.Get_XAAS_WTPM_TrunkPressure(client, redisClient, messageString);
                                }
                                else
                                {
                                    $"读取{job.CommandType}，linkid:{job.LinkId}不存在".Info();
                                }
                            }
                            else
                            {
                                await Vendor.XAAS.Box.Get_XAAS_WTPM_TrunkPressure(client, redisClient, messageString);
                            }
                            break;

                        case "Get_XAAS_WII_WaterInjectingInstrument": //读取注水仪数据

                            if (useLinkId)
                            {
                                var mclient = TcpBridge.ManyClients.Values.FirstOrDefault(c => c.LinkId.HasValue && c.LinkId == job.LinkId);
                                if (mclient != null)
                                {
                                    await Vendor.XAAS.Box.Get_XAAS_WII_WaterInjectingInstrument(client, redisClient, messageString);
                                }
                                else
                                {
                                    $"读取{job.CommandType}，linkid:{job.LinkId}不存在".Info();
                                }
                            }
                            else
                            {
                                await Vendor.XAAS.Box.Get_XAAS_WII_WaterInjectingInstrument(client, redisClient, messageString);
                            }
                            break;

                        case "Post_XAAS_WII_InjectionAllocation": //下发配注量
                            await Vendor.XAAS.Box.Post_XAAS_WII_InjectionAllocation(client, redisClient, messageString);
                            break;

                        case "Post_XAAS_WII_TubePressure": //下发管压设定
                            await Vendor.XAAS.Box.Post_XAAS_WII_TubePressure(client, redisClient, messageString);
                            break;
                        #endregion

                        #region  //延安华圣
                        case "Get_YAHS_WTPM_TrunkPressure": //读取配水间干压

                            var mClient = TcpBridge.ManyClients.Values.FirstOrDefault(c => c.LinkId.HasValue && c.LinkId == job.LinkId);
                            if (mClient != null)
                            {
                                await Vendor.YAHS.Box.Get_YAHS_WTPM_TrunkPressure(client, redisClient, messageString);
                            }
                            else
                            {
                                $"读取{job.CommandType}，linkid:{job.LinkId}不存在".Info();
                            }
                            break;

                        case "Get_YAHS_WII_WaterInjectingInstrument": //读取注水仪数据

                            mClient = TcpBridge.ManyClients.Values.FirstOrDefault(c => c.LinkId.HasValue && c.LinkId == job.LinkId);
                            if (mClient != null)
                            {
                                await Vendor.YAHS.Box.Get_YAHS_WII_WaterInjectingInstrument(client, redisClient, messageString);
                            }
                            else
                            {
                                $"读取{job.CommandType}，linkid:{job.LinkId}不存在".Info();
                            }

                            break;

                        case "Post_YAHS_WII_InjectionAllocation": //下发配注量
                            await Vendor.YAHS.Box.Post_YAHS_WII_InjectionAllocation(client, redisClient, messageString);
                            break;

                        case "Post_YAHS_WII_TubePressure": //下发管压设定
                            await Vendor.YAHS.Box.Post_YAHS_WII_TubePressure(client, redisClient, messageString);
                            break;
                        #endregion

                        #region  //洛阳乾禾
                        case "Get_LYQH_WG_IndicatorDiagram": //读取功图
                            await GT.Get_LYQH_WG_IndicatorDiagram(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_WG_IndicatorDiagramSetting": //设置功图配置
                            await GT.POST_LYQH_WG_IndicatorDiagramSetting(client, redisClient, messageString);
                            break;

                        case "Post_LYQH_KZG_StartWell": // 开井操作
                            await KZG.POST_LYQH_KZG_StartWell(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_KZG_StopWell": // 关井操作
                            await KZG.POST_LYQH_KZG_StopWell(client, redisClient, messageString);
                            break;
                        case "Get_LYQH_DB_PowerMeter": //电表
                            await DB.Get_LYQH_DB_PowerMeter(client, redisClient, messageString);
                            break;
                        case "Get_LYQH_DLT_CurrentDiagram": //电流图
                            await DLT.Get_LYQH_DLT_CurrentDiagram(client, redisClient, messageString);
                            break;
                        case "Get_LYQH_GLT_PowerDiagram": //功率图
                            await GLT.Get_LYQH_GLT_PowerDiagram(client, redisClient, messageString);
                            break;
                        case "Get_LYQH_DYM_Movefluidlevel": //动液面
                            await DYM.Get_LYQH_DYM_Movefluidlevel(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_DYMSetting": //设置动液面
                            await DYM.POST_LYQH_DYMSetting(client, redisClient, messageString);
                            break;

                        case "Get_LYQH_GJL_Tankmeasure": //液位罐计量
                            await GJL.Get_LYQH_GJL_Tankmeasure(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_GJLSetting": //液位罐设置
                            await GJL.POST_LYQH_GJLSetting(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_DBSetting": //电参设置
                            await DB.POST_LYQH_DBSetting(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_GTShiShiCaiJI": //功图实时采集
                            await GT.POST_LYQH_GTShiShiCaiJI(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_DYMShiShiCaiJI": //动液面实时采集
                            await DYM.POST_LYQH_DYMShiShiCaiJI(client, redisClient, messageString);
                            break;

                        case "Get_LYQH_KZG_StartWellTime": //读取开井时间
                            await KZG.Get_LYQH_KZG_StartWellTime(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_DB_ShiShiCaiJI":
                            await DB.POST_LYQH_DB_ShiShiCaiJI(client, redisClient, messageString);
                            break;

                        case "POST_LYQH_GJLShiShiCaiJI":
                            await GJL.POST_LYQH_GJLShiShiCaiJI(client, redisClient, messageString);
                            break;


                        #endregion

                        #region 德汇活塞式流量计
                        case "Get_DEHUI_LLJ": //读取流量计
                            await LLJ.Get_DEHUI_LLJ(client, redisClient, messageString);
                            break;
                        #endregion

                        default:
                            Logger.Error("Default:" + job.CommandType);
                            break;
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Task.Run[1-1]:" + ex.Message);
                Console.WriteLine("Task.Run[1-1]:" + ex.StackTrace);
            }
        }

    }
}

