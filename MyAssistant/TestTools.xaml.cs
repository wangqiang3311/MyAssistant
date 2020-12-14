using Acme.Common;
using HslCommunication.ModBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MongoDB.Driver;
using MyAssistant.Common;
using MyAssistant.ViewModel;
using NLog;
using NLog.Fluent;
using NPOI.HSSF.Record.PivotTable;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using Quartz;
using Quartz.Impl;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.Host;
using ServiceStack.OrmLite;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace MyAssistant
{
    /// <summary>
    /// UpdateManage.xaml 的交互逻辑
    /// </summary>
    public partial class TestTools : Window
    {
        private const string QueudId = "YCIOT";

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public TestTools()
        {
            InitializeComponent();

            #region  创建消息通知
            Logger.Info("测试工具已启动");
            ShowMessage("测试工具已启动");
            #endregion
        }

        private void ShowMessage(string message, int? delay = 5)
        {
            TaskBar bar = new TaskBar();
            bar.Left = SystemParameters.WorkArea.Size.Width - bar.Width;
            bar.Top = SystemParameters.WorkArea.Size.Height - bar.Height;

            bar.Topmost = true;

            bar.txtMessage.Text = message;
            bar.Show();
            if (delay.HasValue)
                bar.CloseMessage(delay.Value);
        }


        /// <summary>
        /// 根据Excel生成sql语句
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MakeSql_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var filePath = System.IO.Path.Combine(executablePathRoot, "DHTable.xlsx");
            IWorkbook workbook = null;
            ISheet sheet = null;
            try
            {
                workbook = WorkbookFactory.Create(filePath);
                sheet = workbook.GetSheetAt(0);//获取第一个工作薄

                var tableName = "";
                var fieldName = "";
                var fieldType = "";

                Dictionary<string, string> fields = new Dictionary<string, string>();

                for (var j = 1; j < 63; j++)
                {
                    var row = sheet.GetRow(j);

                    if (row.Cells.Count == 4)
                    {
                        var v = GetCellValue(row.Cells[0]);
                        tableName = v.ToString();
                        var key = GetCellValue(row.Cells[1]);
                        fieldName = key.ToString();
                        var value = GetCellValue(row.Cells[2]);
                        fieldType = value.ToString();

                        fields.Add(fieldName, fieldType);
                    }
                    else
                    {
                        int i = 0;
                        foreach (var cell in row.Cells)
                        {
                            var v = GetCellValue(cell);
                            if (i == 0)
                            {
                                fieldName = v.ToString();
                            }
                            if (i == 1)
                            {
                                fieldType = v.ToString();
                            }
                            i++;
                        }
                        fields.Add(fieldName, fieldType);
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"CREATE TABLE {tableName} (");
                foreach (var item in fields)
                {
                    if (item.Key == "Id")
                    {
                        sb.AppendLine($"{item.Key} {item.Value}  PRIMARY KEY  AUTO_INCREMENT,");
                    }
                    else
                    {
                        sb.AppendLine($"{item.Key} {item.Value},");
                    }
                }
                sb.AppendLine($")");


                txtResult.Document.Blocks.Clear();

                Paragraph p = new Paragraph();
                Run r = new Run(sb.ToString());
                p.Inlines.Add(r);
                txtResult.Document.Blocks.Add(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取excel数据出错" + ex.Message);
                workbook?.Close();

            }
        }

        protected object GetCellValue(ICell item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            switch (item.CellType)
            {
                case CellType.Boolean:
                    return item.BooleanCellValue;

                case CellType.Error:
                    return ErrorEval.GetText(item.ErrorCellValue);
                case CellType.Formula:
                    switch (item.CachedFormulaResultType)
                    {
                        case CellType.Boolean:
                            return item.BooleanCellValue;

                        case CellType.Error:
                            return ErrorEval.GetText(item.ErrorCellValue);

                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(item))
                            {
                                return item.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                return item.NumericCellValue;
                            }
                        case CellType.String:
                            var str = item.StringCellValue;
                            if (!string.IsNullOrEmpty(str))
                            {
                                return str;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case CellType.Unknown:
                        case CellType.Blank:
                        default:
                            return string.Empty;
                    }
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(item))
                    {
                        return item.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {

                        return item.NumericCellValue;
                    }
                case CellType.String:
                    var strValue = item.StringCellValue;
                    return strValue.Trim();

                case CellType.Unknown:
                case CellType.Blank:
                default:
                    return string.Empty;
            }
        }

        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            SendEmailAsync();
        }
        private async void SendEmailAsync(params string[] fileList)
        {
            string from = "540887384@qq.com";
            string fromer = "wbq3311";
            string to = "540887384@qq.com";
            string toer = "wbq";
            string Subject = "overtcp";

            List<string> files = new List<string>();

            if (fileList.Length == 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.InitialDirectory = @"D:\Desktop\publish";
                ofd.Filter = "(zip压缩文件)|*.z*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == false)
                {
                    MessageBox.Show("没有选择文件");
                    return;
                }

                string[] filePath = ofd.FileNames;

                files.AddRange(filePath);
            }
            else
            {
                files.AddRange(fileList);
            }

            string Body = "传送文件";
            string SMTPHost = "smtp.qq.com";
            string SMTPuser = "540887384@qq.com";
            string SMTPpass = "vkyuhqrejvuobfji";   //qclhpkrldvdzbfib


            List<Task> tasks = new List<Task>();

            bool flag = true;

            int s = 0;
            int f = 0;


            foreach (var item in files)
            {
                var isSuccess = await Sendmail(from, fromer, to, toer, Subject, Body, item, SMTPHost, SMTPuser, SMTPpass);

                if (!isSuccess)
                {
                    f++;
                    flag = false;
                }
                else
                {
                    s++;
                }
                Thread.Sleep(new Random().Next(3000));
            }

            if (flag)

                ShowMessage($"邮件发送完毕，共有{files.Count}个邮件,成功{s}个，失败{f}个");
        }

        private async Task<bool> Sendmail(string sfrom, string sfromer, string sto, string stoer, string sSubject, string sBody, string sfile, string sSMTPHost, string sSMTPuser, string sSMTPpass)
        {
            await DeleteAllMessage();

            ////设置from和to地址
            MailAddress from = new MailAddress(sfrom, sfromer);
            MailAddress to = new MailAddress(sto, stoer);

            ////创建一个MailMessage对象
            MailMessage oMail = new MailMessage(from, to);
            ////邮件标题
            oMail.Subject = sSubject;

            ////邮件内容
            oMail.Body = sBody;

            if (sfile != "")
                //添加附件
                oMail.Attachments.Add(new Attachment(sfile));

            ////邮件格式
            oMail.IsBodyHtml = false;

            ////邮件采用的编码
            oMail.BodyEncoding = Encoding.UTF8;

            ////设置邮件的优先级为高
            oMail.Priority = MailPriority.High;

            ////发送邮件
            SmtpClient client = new SmtpClient();
            client.Host = sSMTPHost;
            client.Port = 587;
            client.Credentials = new NetworkCredential(sSMTPuser, sSMTPpass);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                await client.SendMailAsync(oMail);
                return true;
            }
            catch (Exception err)
            {
                ShowMessage(err.Message.ToString());
                return false;
            }
            finally
            {
                oMail.Dispose();
            }

        }

        private Task<bool> DeleteAllMessage()
        {
            string host = "pop.qq.com";
            string user = "540887384@qq.com";
            string pass = "vkyuhqrejvuobfji";   //qclhpkrldvdzbfib
            int port = 995;

            EmailHelper email = new EmailHelper(user, pass, host, port, true, null);
            return email.DeleteAllMessage();
        }

        private void btnPackage_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            proc.StartInfo.WorkingDirectory = executablePathRoot;
            proc.StartInfo.FileName = "publish.bat";

            var projectRoot = @"D:\project\jiupai\ModbusPoll\src\YCIOT.ModbusPoll.RtuOverTcp";

            proc.StartInfo.Arguments = projectRoot;
            proc.Start();
            proc.WaitForExit();

            //读取发布位置
            var filePath = System.IO.Path.Combine(projectRoot, "Properties", "PublishProfiles", "FolderProfile.pubxml");

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            var nodes = doc.GetElementsByTagName("PublishDir");

            var publishDir = nodes[0].InnerText;

            var dirName = System.IO.Path.GetDirectoryName(publishDir);
            var folderName = System.IO.Path.GetFileName(publishDir);
            var zipFilePath = System.IO.Path.Combine(dirName, folderName + ".zip");
            //压缩文件
            ZipPackage.Zip(publishDir, zipFilePath);
            SendEmailAsync(zipFilePath);
        }

        private async void btnStartJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Grab the Scheduler instance from the Factory 
                IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

                // and start it off
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<HelloJob>()
                   .WithIdentity("job1", "group1")
                   .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
                // some sleep to show what's happening
                Thread.Sleep(TimeSpan.FromSeconds(3));

                // and last shut down the scheduler when you are ready to close your program
                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        private void btnCheckRedis_Click(object sender, RoutedEventArgs e)
        {
            var appSettings = new AppSettings();

            var isHasRedis = true;
            var redisclient = appSettings.GetString("Redis.WriterData");
            var otherRedisClient = new RedisClient();

            if (redisclient is null)
                isHasRedis = false;
            else
                otherRedisClient = new RedisClient(redisclient);
            var isPushData = false;
            var YTGSRedisClient = new RedisClient();
            var redisPushclient = appSettings.GetString("Redis.SyncPush");//redis://clientid:ERe@3_rit!@10.30.2.77:6379?db=15
            if (redisPushclient is null)
            {
                isPushData = false;
            }
            else
            {
                isPushData = true;
                YTGSRedisClient = new RedisClient(redisPushclient);
            }

            DateTime start = DateTime.Now;
            int period = appSettings.Get<int>("RedisCheckPeriod", 30000);

            Task.Run(() =>
            {
                while (true)
                {
                    DateTime end = DateTime.Now;

                    if (end.Subtract(start).TotalMilliseconds >= period)
                    {
                        if (redisclient != null)
                        {
                            isHasRedis = IsOnline(redisclient);

                            if (isHasRedis == false)
                            {
                                otherRedisClient?.Dispose();
                                //重新实例化redis
                                otherRedisClient = new RedisClient(redisclient);
                                start = DateTime.Now;
                                isHasRedis = IsOnline(redisclient);
                                Console.WriteLine("重新实例化CaiYouRedisClient");
                            }
                            else
                            {
                                Console.WriteLine("CaiYouRedisClient:" + isHasRedis);
                            }

                        }
                        if (redisPushclient != null)
                        {
                            isPushData = IsOnline(redisPushclient);
                            if (isPushData == false)
                            {
                                YTGSRedisClient?.Dispose();
                                //重新实例化redis
                                YTGSRedisClient = new RedisClient(redisPushclient);
                                start = DateTime.Now;
                                Console.WriteLine("重新实例化YCGSRedisClient");
                                isPushData = IsOnline(redisPushclient);
                            }
                            else
                            {
                                Console.WriteLine("YCGSRedisClient:" + isPushData);
                            }
                        }

                    }

                }
            });
        }

        private static bool IsOnline(string baseUrl, int millisecondsTimeout = 2000)
        {
            try
            {
                if (string.IsNullOrEmpty(baseUrl)) return false;

                var redisClientUrl = GetUrl(baseUrl);

                var urlSegments = redisClientUrl.Split(':');
                if (urlSegments.Length == 2)
                {
                    var isOnline = TcpClientConnector.IsOnline(urlSegments[0], int.Parse(urlSegments[1]), millisecondsTimeout);
                    return isOnline;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GetUrl(string client)
        {
            string url = client;

            if (string.IsNullOrEmpty(client)) return url;

            var linkString = client.Split('@');
            if (linkString.Length > 1)
            {
                var clientip = linkString.Last().Split('?');//10.30.2.77:6379?db=15
                if (clientip.Length == 2)
                {
                    url = clientip[0];
                }
            }
            return url;
        }

        private void btnManualCheckRedis_Click(object sender, RoutedEventArgs e)
        {
            var appSettings = new AppSettings();

            var isHasRedis = true;
            var redisclient = appSettings.GetString("Redis.WriterData");
            var otherRedisClient = new RedisClient();

            if (redisclient is null)
                isHasRedis = false;
            else
                otherRedisClient = new RedisClient(redisclient);
            var isPushData = false;
            var YTGSRedisClient = new RedisClient();
            var redisPushclient = appSettings.GetString("Redis.SyncPush");//redis://clientid:ERe@3_rit!@10.30.2.77:6379?db=15
            if (redisPushclient is null)
            {
                isPushData = false;
            }
            else
            {
                isPushData = true;
                YTGSRedisClient = new RedisClient(redisPushclient);
            }
            if (isHasRedis)
            {
                isHasRedis = IsOnline(redisclient);
                Console.WriteLine("CaiYouRedisClient:" + isHasRedis);
            }
            if (isPushData)
            {
                isPushData = IsOnline(redisPushclient);
                Console.WriteLine("YCGSRedisClient:" + isPushData);
            }
        }

        private void btnCheckUrl_Click(object sender, RoutedEventArgs e)
        {
            var url = this.txtUrl.Text;
            var isOn = IsOnline(url);
            MessageBox.Show($"{url} {isOn}");
        }

        private void btnReadRedis_Click(object sender, RoutedEventArgs e)
        {
            var appSettings = new AppSettings();
            var redisCon = appSettings.GetString("TestRedis");
            var groupName = appSettings.GetString("GroupName");

            RedisClient redisClient = null;
            try
            {
                redisClient = new RedisClient(redisCon);
                var userRequestListId = $"{QueudId}:User:JobList:{groupName}";

                var messageString = redisClient.RemoveStartFromList(userRequestListId);
                Console.WriteLine("读取到UserJob:" + messageString.Dump());

                var quartzRequestListId = $"{QueudId}:Quartz:JobList:" + groupName;
                messageString = redisClient.RemoveStartFromList(quartzRequestListId);
                Console.WriteLine("读取到QuartzJob:" + messageString.Dump());
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取redis异常" + ex.Message + "\n" + ex.StackTrace);
                redisClient = new RedisClient(redisCon);
                Console.WriteLine("重新实例化redis");
            }
        }

        private void btnGetOilData_Click(object sender, RoutedEventArgs e)
        {
            var connectionFactory = App.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            using var dbFac = connectionFactory.OpenDbConnection();

            var txtWell = this.txtWell.Text;

            IotOilWell oilWell = null;
            if (Regex.IsMatch(txtWell, @"^\d+$"))
            {
                oilWell = dbFac.Single<IotOilWell>(w => w.WellId == long.Parse(txtWell));
            }
            else
            {
                oilWell = dbFac.Single<IotOilWell>(w => w.WellName == txtWell);
            }

            if (oilWell != null)
            {
                var wellId = oilWell.WellId;

                var wells = dbFac.Select<IotOilWellDevice>(w => w.WellId == wellId);

                var deviceTypes = dbFac.Select<IotDeviceType>();


                List<string> results = new List<string>();
                foreach (var well in wells)
                {
                    var deviceType = deviceTypes.SingleOrDefault(d => d.Id == well.DeviceTypeId);

                    var o = new
                    {
                        wellId,
                        oilWell.WellName,
                        well.GroupName,
                        well.ModbusAddress,
                        well.SlotId,
                        well.LinkId,
                        well.RemoteHost,
                        deviceType.Name,
                        well.NetworkNode
                    };
                    results.Add(o.ToJson().IndentJson());
                }
                WriteToResult(results);
            }
        }

        private void WriteToResult(List<string> contents)
        {
            txtResult.Document.Blocks.Clear();

            foreach (var item in contents)
            {
                Paragraph p = new Paragraph();
                Run r = new Run(item);
                p.Inlines.Add(r);
                txtResult.Document.Blocks.Add(p);
            }
        }

        private void btnWriteToReids_Click(object sender, RoutedEventArgs e)
        {
            var appSettings = new AppSettings();
            var redisCon = appSettings.GetString("TestRedis");

            using var redisClient = new RedisClient(redisCon);

            var json = this.txtJson.Text;
            var listId = this.txtListId.Text;

            redisClient.AddItemToList(listId, json.IndentJson());
        }

        private void btnUnpack_Click(object sender, RoutedEventArgs e)
        {
            GuoYi(this.txtUnpackageForWater.Text);
        }

        public void GuoYi(string hex)
        {
            var s = HexStringToBytes(hex);
            var results = GuoYiWaterInject(s);

            WriteToResult(results);
        }

        public List<string> GuoYiWaterInject(byte[] data)
        {
            List<string> results = new List<string>();
            var client = new ModbusRtuOverTcp();

            var value8 = client.ByteTransform.TransUInt16(data, 8);
            var value6 = client.ByteTransform.TransUInt16(data, 6);

            var cumulativeFlow = (value6 << 16 | value8) / 100.0;
            results.Add($"表头累计：{cumulativeFlow}");

            var value = client.ByteTransform.TransInt16(data, 2);
            var settedFlow = value / 100.0; //设定流量回读

            results.Add($"流量回读：{settedFlow}");


            value = client.ByteTransform.TransInt16(data, 14);
            var tubePressure = value / 100.0;//管压

            results.Add($"管压：{tubePressure}");
            value = client.ByteTransform.TransInt16(data, 4);
            var instantaneousFlow = value / 100.0; //瞬时流量

            results.Add($"瞬时流量：{instantaneousFlow}");


            value = client.ByteTransform.TransInt16(data, 0);
            var valveStatus = (value >> 4) & 0x01; //阀门状态
            var valveMode = (value >> 1) & 0x01; //阀门工作模式

            return results;
        }


        public byte[] HexStringToBytes(string hexString)
        {
            string[] array = hexString.Split(' ');

            List<byte> bytes = new List<byte>();

            string[] hex = { "A", "B", "C", "D", "E", "F" };

            foreach (var item in array)
            {
                if (item.StartsWith("0"))
                {
                    var newValue = item.TrimStart('0');

                    if (hex.Contains(newValue))
                    {
                        switch (newValue)
                        {
                            case "A":
                                newValue = "10";
                                break;
                            case "B":
                                newValue = "11";
                                break;
                            case "C":
                                newValue = "12";
                                break;
                            case "D":
                                newValue = "13";
                                break;
                            case "E":
                                newValue = "14";
                                break;
                            case "F":
                                newValue = "15";
                                break;
                        }
                        bytes.Add(byte.Parse(newValue));
                    }
                    else
                    {
                        if (newValue == "") newValue = "0";
                        var value1 = byte.Parse(newValue);

                        if (value1 < 9)
                        {
                            bytes.Add(value1);
                        }
                    }
                }
                else
                {
                    var chars = item.ToCharArray();

                    var first = chars[0].ToString();

                    if (hex.Contains(first))
                    {
                        switch (first)
                        {
                            case "A":
                                first = "10";
                                break;
                            case "B":
                                first = "11";
                                break;
                            case "C":
                                first = "12";
                                break;
                            case "D":
                                first = "13";
                                break;
                            case "E":
                                first = "14";
                                break;
                            case "F":
                                first = "15";
                                break;
                        }
                    }


                    var second = chars[1].ToString();

                    if (hex.Contains(second))
                    {
                        switch (second)
                        {
                            case "A":
                                second = "10";
                                break;
                            case "B":
                                second = "11";
                                break;
                            case "C":
                                second = "12";
                                break;
                            case "D":
                                second = "13";
                                break;
                            case "E":
                                second = "14";
                                break;
                            case "F":
                                second = "15";
                                break;
                        }
                    }

                    var high = byte.Parse(first);
                    var low = byte.Parse(second);

                    var value1 = high << 4 | low;
                    bytes.Add((byte)value1);
                }
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// 打包发布v2所有新程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPackageNew_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            PublishV2(Convert.ToInt32(b.CommandParameter));
        }
        /// <summary>
        /// 打包发布
        /// </summary>
        /// <param name="handleLevel">处理级别0：不处理；1：删除config和db；2：删除旧文件，包括1的处理</param>
        private void PublishV2(int handleLevel = 0)
        {
            Dictionary<string, string> projects = new Dictionary<string, string>();

            var modbusRoot = @"D:\project\jiupai\ModbusPoll\src\YCIOT.ModbusPoll.RtuOverTcp";
            var jobRoot = @"D:\project\jiupai\YCBZ_V2\src\YCIOT_V2\Executor\YCBZ_V2.Job";
            var datawriterRoot = @"D:\project\jiupai\YCBZ_V2\src\YCIOT_V2\Executor\YCBZ_V2.DataProcess";
            var v2Root = @"D:\project\jiupai\YCBZ_V2\src\YCIOT_V2\YCBZ_V2.SubCenter\YCBZ_V2.Service";
            var web = @"D:\project\jiupai\YCBZ_V2\src\YCBZ_V2\YCBZ_V2.SubCenter";
            var app = @"D:\project\jiupai\YCBZ_V2\src\YCIOT_V2\YCBZ_V2.App\YCBZ_V2.App.Server";

            var desDir = @"D:\Desktop\publish";
            FileHelper.DeleteDir(desDir);

            projects.Add("modbus", modbusRoot);
            projects.Add("job", jobRoot);
            projects.Add("datawriter", datawriterRoot);
            projects.Add("v2", v2Root);
            projects.Add("web", web);
            projects.Add("app", app);


            string publishDir = "";

            foreach (var item in projects)
            {
                Console.WriteLine($"{item.Key}打包开始");

                if (item.Key == "v2")
                {
                    //用release发布，用配置文件发布不成功

                    PublishOne(item.Value, "publishV2");

                    publishDir = System.IO.Path.Combine(item.Value, @"bin\release\netcoreapp3.1");
                }
                else if (item.Key == "web")
                {
                    PublishOne(web, "publishweb");

                    Console.WriteLine($"准备移动前端dist");
                    PublishOne(web, "move");
                    Console.WriteLine($"移动前端dist结束");

                    publishDir = "";
                }
                else
                {
                    PublishOne(item.Value);

                    //读取发布位置
                    var filePath = System.IO.Path.Combine(item.Value, "Properties", "PublishProfiles", "FolderProfile.pubxml");

                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);

                    var nodes = doc.GetElementsByTagName("PublishDir");

                    if (nodes.Count == 0)
                    {
                        nodes = doc.GetElementsByTagName("PublishUrl");
                    }

                    publishDir = nodes[0].InnerText;
                }

                if (publishDir != "")
                {
                    if (handleLevel > 0)
                        Handle(handleLevel, publishDir);
                }

                if (item.Key == "v2")
                {
                    Console.WriteLine($"准备移动v2 release");
                    PublishOne(item.Value, "moveV2");
                    Console.WriteLine($"移动v2 release结束");
                }

                Console.WriteLine($"{item.Key}打包结束");
            }

            //压缩包
            var desDirName = System.IO.Path.GetFileName(desDir);

            var desParent = System.IO.Directory.GetParent(desDir);

            var zipFilePath = System.IO.Path.Combine(desParent.FullName, desDirName + ".zip");
            //压缩文件
            ZipPackage.Zip(desDir, zipFilePath);

            Console.WriteLine("打包结束");
        }

        private static void Handle(int handleLevel, string publishDir)
        {
            var files = Directory.GetFiles(publishDir);

            foreach (var sourceFile in files)
            {
                var extension = System.IO.Path.GetExtension(sourceFile);
                if (extension == ".pdb" || extension == ".config")
                {
                    System.IO.File.Delete(sourceFile);
                }
            }

            //复杂处理，删除掉旧文件,只留下最新dll文件
            if (handleLevel == 2)
            {
                //如果是文件夹删除
                FileHelper.DeleteAllSubDirectories(publishDir);

                //如果是上月的文件删除

                FileHelper.DeleteOldFiles(publishDir);
            }
        }




        private static void PublishOne(string projectRoot, string bat = "publish")
        {
            Process proc = new Process();
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            proc.StartInfo.WorkingDirectory = executablePathRoot;
            proc.StartInfo.FileName = $"{bat}.bat";

            proc.StartInfo.Arguments = projectRoot;
            proc.Start();
            proc.WaitForExit();
        }
    }

    public class HelloJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                //操作数据库
                var connectionFactory = App.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
                using var dbFac = connectionFactory.OpenDbConnection();

                var settings = App.ServiceProvider.GetRequiredService<IBookstoreDatabaseSettings>();
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);

                var books = database.GetCollection<Book>(settings.BooksCollectionName);

                Console.WriteLine("Greetings from HelloJob!");
            });
        }
    }
}