using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HslCommunication.ModBus;
using YCIOT.ModbusPoll.RtuOverTcp.Utils;
using YCIOT.ModbusPoll.RtuOverTcp;
using YCIOT.ServiceModel.IOT;
using Acme.Common.Utils;
using Acme.Common;

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.WAGL
{
    // ReSharper disable once InconsistentNaming
    public static class WM3000WXGT
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Get_XAGL_WM3000WXGT_IndicatorDiagram(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
        {
            var par = messageString.FromJson<ControlRequest>();

            try
            {
                var logIotModbusPoll = par.ConvertTo<LogIotModbusPoll>();

                logIotModbusPoll.State = 0;
                logIotModbusPoll.Result = "ok";

                var flag = true;



                var indicatorDiagram = new IotDataOilWellIndicatorDiagram()
                {
                    NetworkNode = ClientInfo.ManyIpAddress,
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);

                indicatorDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                var modbusAddress = par.ModbusAddress;
                ClientInfo.CurrentModbusPoolAddress = modbusAddress;
                var commandParameter = par.CommandParameter.ToString();
                var Slot = Convert.ToInt32(jo1[par.DeviceId.ToString()].ToString());

                //try
                //{
                //从api中获取值
                var host = par.RemoteHost;
                var url = "/getworkgraph/";
                bool IsHostAlive = false;

                if (host.StartsWith("192"))
                {
                    $"无线功图树莓派({host})不正确，或者没有接入无线功图".Error();
                }
                else
                {
                    IsHostAlive = new TcpClientConnector().isOnline(host, null, 1000);
                    if (IsHostAlive)
                    {
                        url = $"http://{host}{url}{Slot}";

                        $"请求树莓派api:{url}".Info();

                        var indicatorDiagramJson = await url.GetJsonFromUrlAsync();

                        $"树莓派api返回数据:{indicatorDiagramJson}".Info();

                        var oilWellIndicatorDiagram = indicatorDiagramJson.FromJson<OilWellIndicatorDiagram>();

                        indicatorDiagram.Stroke = oilWellIndicatorDiagram.chongCheng;
                        indicatorDiagram.L = oilWellIndicatorDiagram.load;
                        indicatorDiagram.D = oilWellIndicatorDiagram.displace;



                        var maxDis = indicatorDiagram.D.Max();
                        var minDis = indicatorDiagram.D.Min();

                        if (!indicatorDiagram.Displacement.HasValue)
                        {
                            indicatorDiagram.Displacement = maxDis;
                        }

                        for (var i = 0; i < indicatorDiagram.D.Count; i++)
                        {
                            if (Math.Abs(maxDis - minDis) > 0.1)
                            {
                                indicatorDiagram.D[i] = Math.Round(((indicatorDiagram.D[i] - minDis) / (maxDis - minDis) * (double)indicatorDiagram.Displacement), 2);
                            }
                        }

                        //补点
                        if (indicatorDiagram.L.Count > 0)
                        {
                            indicatorDiagram.L.Add(indicatorDiagram.L[0]);
                        }
                        if (indicatorDiagram.D.Count > 0)
                        {
                            indicatorDiagram.D.Add(indicatorDiagram.D[0]);
                        }

                        indicatorDiagram.MaxLoad = indicatorDiagram.L.Max();
                        indicatorDiagram.MinLoad = indicatorDiagram.L.Min();
                        indicatorDiagram.AvgLoad = indicatorDiagram.L.Average();


                        indicatorDiagram.DateTime = DateTime.Now;  //树莓派时间不准确，取服务器时间
                                                                   //to do 树莓派时间稳定了再换
                        indicatorDiagram.WellId = oilWellIndicatorDiagram.wellId;
                        indicatorDiagram.Count = oilWellIndicatorDiagram.count;
                        indicatorDiagram.Interval = oilWellIndicatorDiagram.interval;

                        indicatorDiagram.DeviceTypeId = par.DeviceTypeId;
                        indicatorDiagram.Mock = par.UseMockData;
                        indicatorDiagram.Displacement = Convert.ToDouble(jo1["0"].ToString());

                        indicatorDiagram.NetworkNode = host;

                        redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());
                        redisClient.Set($"Group:OilWell:{par.DeviceName}-{par.DeviceId}:IndicatorDiagram", indicatorDiagram);
                        redisClient.Set($"Single:OilWell:IndicatorDiagram:{par.DeviceName}-{par.DeviceId}", indicatorDiagram);
                    }
                    else
                    {
                        $"无线功图树莓派({host})网络不通!".Error();
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("无线功图从api中获取异常：" + ex.Message + "," + ex.StackTrace);
                //}

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

        public class OilWellIndicatorDiagram
        {
            public long wellId { set; get; }
            public DateTime dateTime { get; set; }   //数据采集时间
            public double? chongCheng { get; set; } //冲次
            public double? interval { get; set; }    //采样间隔
            public int? count { get; set; }          //采样点数
            public List<double> displace { get; set; }     //位移
            public List<double> load { get; set; }     //载荷
        }
    }
}
