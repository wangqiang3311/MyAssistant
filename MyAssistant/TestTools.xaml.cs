using Microsoft.Win32;
using MyAssistant.Common;
using MyAssistant.ViewModel;
using NLog.Fluent;
using NPOI.HSSF.Record.PivotTable;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Host;
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

namespace MyAssistant
{
    /// <summary>
    /// UpdateManage.xaml 的交互逻辑
    /// </summary>
    public partial class TestTools : Window
    {
        public static IAppSettings appSettings = new AppSettings();
        public static string yanchangRoot;
        public static string ansenRoot;
        public static string yanchangs;
        public static string ansens;

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

        private async void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            string from = "540887384@qq.com";
            string fromer = "wbq3311";
            string to = "540887384@qq.com";
            string toer = "wbq";
            string Subject = "overtcp";

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = @"D:\";
            ofd.Filter = "(zip压缩文件)|*.z*";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == false)
            {
                MessageBox.Show("没有选择文件");
                return;
            }

            string[] filePath = ofd.FileNames;

            List<string> files = new List<string>();
            files.AddRange(filePath);

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

        private void btnReceiveEmail_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string targetFolder = "Update";
            var targetPath = System.IO.Path.Combine(executablePathRoot, targetFolder);
            DeleteDir(targetPath);
            Task.Run(() =>
            {
                //每3秒，检查邮件
                Thread.Sleep(1000);
                string host = "pop.qq.com";
                string user = "540887384@qq.com";
                string pass = "vkyuhqrejvuobfji";   //qclhpkrldvdzbfib
                int port = 995;

                //解压到Update文件夹下

                //string root = @"D:\Desktop\download";

                EmailHelper email = new EmailHelper(user, pass, host, port, true, null);
                string error = "";
                var isSuccess = email.ValidateAccount(ref error);

                if (isSuccess)
                {
                    var count = email.GetEmailCount();

                    if (count > 0)
                    {
                        var zipFilePath = "";
                        for (int i = count; i >= 1; i--)
                        {
                            //收取附件
                            var result = email.DownAttachmentsById(targetPath, i);

                            if (result.Item1)
                            {
                                if (result.Item2 != "")
                                {
                                    zipFilePath = result.Item2;
                                }
                                if (zipFilePath != "" && i == 1)
                                    //解压到同名目录下
                                    UnPackage.Unzip(zipFilePath, targetPath);
                                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                {
                                    ShowMessage($"收取文件成功,当前MessageId：{i}");
                                });
                            }
                        }
                    }
                }

            });
        }
        /// <summary>
        /// 写入redis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteToRedis_Click(object sender, RoutedEventArgs e)
        {

            using var redisClient = new RedisClient("112.126.101.120", 6379, "ERe@3_rit!", 14);

            string QueueId = "YCIOT";

            var jobParameter = "[{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000003,\"DeviceName\":\"1-3井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":51,\"DeviceTypeId\":1,\"CommandParameter\":{\"13\":1005024,\"14\":1000523,\"17\":1005125,\"16\":1005206}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000013,\"DeviceName\":\"1-9井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":52,\"DeviceTypeId\":1,\"CommandParameter\":{\"110\":1004920,\"112\":1004922,\"113\":1004923,\"116\":1004998}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000004,\"DeviceName\":\"1-51井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":54,\"DeviceTypeId\":1,\"CommandParameter\":{\"151\":1005106,\"152\":1005107,\"153\":1005108,\"158\":1005113}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000006,\"DeviceName\":\"1-68井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":55,\"DeviceTypeId\":1,\"CommandParameter\":{\"170\":1005123,\"171\":1005126,\"172\":1005127}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000009,\"DeviceName\":\"1-74井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":56,\"DeviceTypeId\":1,\"CommandParameter\":{\"175\":1005129,\"177\":1005131,\"179\":1005133,\"173\":1005205}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9000011,\"DeviceName\":\"1-80井场\",\"StationId\":12101010009000,\"StationName\":\"1-27工作站\",\"ModbusAddress\":57,\"DeviceTypeId\":1,\"CommandParameter\":{\"183\":1005207,\"182\":1005212,\"180\":1005213,\"184\":1005214,\"185\":1005215,\"187\":1005216}},{\"JobName\":\"读取油井计量\",\"GroupName\":\"SLS-01\",\"DeviceId\":9002018,\"DeviceName\":\"吴46-37井场\",\"StationId\":12101010009002,\"StationName\":\"46-37工作站\",\"ModbusAddress\":61,\"DeviceTypeId\":1,\"CommandParameter\":{\"4642\":1007553,\"4640\":1007555,\"4637\":1007557,\"4639\":1007663}}]";
            var controlRequests = jobParameter.FromJson<List<ControlRequestDeHui>>();

            foreach (var item in controlRequests)
            {
                if (item.ModbusAddress == 51)
                {
                    item.RequestTime = DateTime.Now;
                    item.CommandType = $"Get_DEHUI_LLJ";
                    item.LinkMode = "RTU";

                    item.UserId = "1";
                    item.UserName = "wbq";

                    item.RemoteHost = "192.168.207.216";
                    item.RemotePort = 502;

                    var listId = $"{QueueId}:User:JobList:" + item.GroupName;

                    redisClient.AddItemToList(listId, item.ToJson().IndentJson());
                    break;
                }
            }
        }
        public static void DeleteDir(string file)
        {
            if (!Directory.Exists(file))
            {
                return;
            }
            try
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            File.Delete(f);
                        }
                        else
                        {
                            DeleteDir(f);
                        }
                    }

                    //删除空文件夹
                    Directory.Delete(file);
                }

            }
            catch (Exception ex) // 异常处理
            {
                Console.WriteLine(ex.Message.ToString());// 异常信息
            }
        }
    }
}