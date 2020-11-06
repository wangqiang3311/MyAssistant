using System;
using System.Net;
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

// ReSharper disable once CheckNamespace
namespace YCIOT.ModbusPoll.Vendor.LYQH
{
     public static class KZG
     {
          private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
          public static async Task POST_LYQH_KZG_controllerstate(ModbusRtuOverTcp client, RedisClient redisClient, string messageString, int state = 1)
          {
               var par = messageString.FromJson<ControlRequest>();
               try
               {
                    var modbusAddress = par.ModbusAddress;

                    // 0 - 关井，1 - 开井
                    //写入1值到0x0080(Holding register)寄存器，设置油井的状态为开启或者关闭

                    var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);

                    var slotId = Convert.ToInt32(jo1["1"].ToString());

                    var startAddress = (ushort)((slotId * 0x100) + 0x0080);

                    var read = await client.WriteAsync($"s={par.ModbusAddress};{startAddress}", new byte[] { 00, (byte)state, 20, 07, 23, 11, 34, 00 });

                    if (read.IsSuccess)
                    {

                    }
                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, read.IsSuccess ? 0 : -2, "ok");
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

          public static Task POST_LYQH_KZG_StartWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               return POST_LYQH_KZG_controllerstate(client, redisClient, messageString, 1);
          }

          public static Task POST_LYQH_KZG_StopWell(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               return POST_LYQH_KZG_controllerstate(client, redisClient, messageString, 0);
          }

          public static async Task Get_LYQH_KZG_StartWellTime(ModbusRtuOverTcp client, RedisClient redisClient, string messageString)
          {
               var par = messageString.FromJson<ControlRequest>();
               try
               {
                    var modbusAddress = par.ModbusAddress;

                    // 读取0x0081  开井时间

                    var jo1 = (JObject)JsonConvert.DeserializeObject(par.CommandParameter);

                    var slotId = Convert.ToInt32(jo1["1"].ToString());

                    var startAddress = (ushort)((slotId * 0x100) + 0x0081);

                lock (ClientInfo.locker)
                {
                    ClientInfo.RequestTime = DateTime.Now;
                    ClientInfo.ExpectedType = 0x03;
                    ClientInfo.ExpectedDataLen = 4 * 2;
                }
                var read = await client.ReadAsync($"s={par.ModbusAddress};3;{startAddress}", 4);

                    if (read.IsSuccess)
                    {
                         var year = client.ByteTransform.TransByte(read.Content, 0);
                         var month = client.ByteTransform.TransByte(read.Content, 1);
                         var date = client.ByteTransform.TransByte(read.Content, 2);
                         var h = client.ByteTransform.TransByte(read.Content, 3);
                         var m = client.ByteTransform.TransByte(read.Content, 4);
                         var s = client.ByteTransform.TransByte(read.Content, 5);

                         var DateTime = new DateTime(2000 + year, month, date, h, m, s);

                    }

                    if (!par.UserName.IsNullOrEmpty())
                    {
                         ServerEventHelper.SendSseMessage(par.UserName, par.SessionId, read.IsSuccess ? 0 : -2, "ok");
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


     }
}
