using Acme.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MongoDB.Driver;
using MyAssistant.Common;
using MyAssistant.ViewModel;
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
        public TestTools()
        {
            InitializeComponent();

            #region  创建消息通知
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

            var txtWellName = this.txtWell.Text;

            var oilWell = dbFac.Single<IotOilWell>(w => w.WellName == txtWellName);

            if (oilWell != null)
            {
                var wellId = oilWell.WellId;

                var wells = dbFac.Select<IotOilWellDevice>(w => w.WellId == wellId);

                foreach (var well in wells)
                {
                    if (well.NetworkNode != well.RemoteHost)
                    {
                        this.txtWell.Text = $"wellId: {wellId}\n WellName: {oilWell.WellName}\n GroupName: {well.GroupName} \n ModusAddress: {well.ModbusAddress}\n SlotId: {well.SlotId}";
                        break;
                    }
                }
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