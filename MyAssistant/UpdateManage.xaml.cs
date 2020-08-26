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
    public partial class UpdateManage : Window
    {
        public static IAppSettings appSettings = new AppSettings();
        public static string yanchangRoot;
        public static string ansenRoot;
        public static string yanchangs;
        public static string ansens;

        public static string exeName = "YCIOT.ModbusPoll.RtuOverTcp";

        public ObservableCollection<ProjectViewModel> projectlist = new ObservableCollection<ProjectViewModel>();
        public UpdateManage()
        {
            InitializeComponent();

            #region  创建消息通知
            ShowMessage("项目发布管理已启动");

            #endregion

            this.ItemProject.ItemsSource = projectlist;

            var assembliesWithServices = new Assembly[1];
            assembliesWithServices[0] = typeof(AppHost).Assembly;
            var appHost = new AppHost("AppHost", assembliesWithServices);

            //从配置中加载
            yanchangRoot = appSettings.Get<string>("RootYanChang");
            ansenRoot = appSettings.Get<string>("RootAnSen");
            yanchangs = appSettings.Get<string>("39.102.46.137");
            ansens = appSettings.Get<string>("iot.sxycyt.net");


            var range = yanchangs.Split('-');

            for (int i = int.Parse(range[0]); i <= int.Parse(range[1]); i++)
            {
                projectlist.Add(new ProjectViewModel()
                {
                    Id = i,
                    Name = i.ToString(),
                    Description = "",
                    TypeId = 1
                });
            }

            range = ansens.Split('-');

            for (int i = int.Parse(range[0]); i <= int.Parse(range[1]); i++)
            {
                projectlist.Add(new ProjectViewModel()
                {
                    Id = i,
                    Name = i.ToString(),
                    Description = "",
                    TypeId = 2
                });
            }

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
        /// 批量更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void batUpdate_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDll = $"{exeName}.dll";
            string sourceDllPath = System.IO.Path.Combine(executablePathRoot, targetDll);

            var exeList = GetExeList(exeName);
            try
            {
                //停掉exe进程
                var t1 = StopProcessAsync(exeName);

                var t2 = t1.ContinueWith(t =>
                  {
                      if (t.Result)
                      {
                          NotifyStop();
                          //执行核心工作
                          return Excute(sourceDllPath, targetDll);
                      }
                      else
                      {
                          return false;
                      }
                  });

                var t3 = t2.ContinueWith(async t =>
                {
                    var result = await StartProcessAsync(exeName, exeList);
                    if (result)
                    {
                        NotifyStart();

                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                            {
                                ShowMessage($"批量更新{exeList.Count}个成功");
                            });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, null);
            }
        }

        private void NotifyStop()
        {
            foreach (var item in projectlist)
            {
                if (item.IsEnable)
                {
                    item.IsStarted = false;
                }
            }

        }
        private void NotifyStop(ProjectViewModel model)
        {
            model.IsStarted = false;
        }


        private void NotifyStart()
        {
            foreach (var item in projectlist)
            {
                if (item.IsEnable)
                {
                    item.IsStarted = true;
                }
            }
        }
        private void NotifyStart(ProjectViewModel model)
        {
            model.IsStarted = true;
        }

        private async Task<bool> StartProcessAsync(string exeName, List<string> exeList)
        {
            //开启exe进程
            var result = StartExe(exeList);

            if (result)
            {
                int count = 5;
                bool isOverTime = false;
                //监测进程数量
                while (GetProcessCount(exeName) < exeList.Count)
                {
                    count--;
                    if (count < 0)
                    {
                        isOverTime = true;
                        break;
                    }
                    await Task.Delay(3000);
                }
                return !isOverTime;
            }
            return false;
        }

        private async Task<bool> StartProcessAsync(string exeName, string exePath)
        {
            //开启exe进程
            var result = StartExe(exePath);

            if (result)
            {
                int count = 5;
                bool isOverTime = false;
                //监测进程数量
                while (GetProcessCount(exeName) < 1)
                {
                    count--;
                    if (count < 0)
                    {
                        isOverTime = true;
                        break;
                    }
                    await Task.Delay(3000);
                }
                return !isOverTime;
            }
            return false;
        }


        private bool Excute(string sourceDllPath, string targetDll)
        {
            //获取当前运行目录下的dll，更新到指定地方
            try
            {
                foreach (var item in projectlist)
                {
                    var root = GetTargetRoot(item);

                    var targetDllPath = System.IO.Path.Combine(root, item.Name, targetDll);

                    if (File.Exists(targetDllPath))
                        System.IO.File.Copy(sourceDllPath, targetDllPath, true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        private bool ExcuteBat(string sourceDir)
        {
            //获取当前运行目录下的所有文件，更新到指定地方
            try
            {
                foreach (var item in projectlist)
                {
                    var root = GetTargetRoot(item);

                    var targetDir = System.IO.Path.Combine(root, item.Name);

                    if (Directory.Exists(targetDir))
                    {
                        var files = Directory.GetFiles(sourceDir);

                        foreach (var sourceFile in files)
                        {
                            var fileName = System.IO.Path.GetFileName(sourceFile);
                            var extension = System.IO.Path.GetExtension(sourceFile);
                            var targetPath = System.IO.Path.Combine(targetDir, fileName);

                            if (extension == ".pdb" || extension == ".config") continue;
                            System.IO.File.Copy(sourceFile, targetPath, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        private bool ExcuteBat(string sourceDir, ProjectViewModel item)
        {
            //获取当前运行目录下的所有文件，更新到指定地方
            try
            {
                var root = GetTargetRoot(item);

                var targetDir = System.IO.Path.Combine(root, item.Name);

                if (Directory.Exists(targetDir))
                {
                    var files = Directory.GetFiles(sourceDir);

                    foreach (var sourceFile in files)
                    {
                        var fileName = System.IO.Path.GetFileName(sourceFile);
                        var extension = System.IO.Path.GetExtension(sourceFile);
                        var targetPath = System.IO.Path.Combine(targetDir, fileName);

                        if (extension == ".pdb" || extension == ".config") continue;

                        System.IO.File.Copy(sourceFile, targetPath, true);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }


        private bool Excute(string sourceDllPath, string targetDll, ProjectViewModel item)
        {
            //获取当前运行目录下的dll，更新到指定地方
            try
            {
                var root = GetTargetRoot(item);
                var targetDllPath = System.IO.Path.Combine(root, item.Name, targetDll);

                if (File.Exists(targetDllPath))
                    System.IO.File.Copy(sourceDllPath, targetDllPath, true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }


        private void KillProcess(string procName)
        {
            var killprocess = Process.GetProcessesByName(procName);
            foreach (var p in killprocess)
            {
                p.Kill();
                p.WaitForExit();
            }
        }
        private void KillProcess(string procName, string targetExePath)
        {
            var killprocess = Process.GetProcessesByName(procName);
            foreach (var p in killprocess)
            {
                if (p.MainWindowHandle == IntPtr.Zero) continue;

                if (p.MainModule.FileName == targetExePath)
                {
                    p.Kill();
                    p.WaitForExit();
                }
            }
        }

        private bool StartExe(List<string> exeList)
        {
            foreach (var item in exeList)
            {
                Task.Run(() =>
                {
                    Process p = Process.Start(item);
                    p.WaitForExit();//关键，等待外部程序退出后才能往下执行
                });
            }
            return true;
        }

        private bool StartExe(string exePath)
        {
            Task.Run(() =>
            {
                Process p = Process.Start(exePath);
                p.WaitForExit();//关键，等待外部程序退出后才能往下执行
            });

            return true;
        }


        private int GetProcessCount(string procName)
        {
            return Process.GetProcessesByName(procName).Length;
        }
        private int GetProcessCount(string procName, string procPath)
        {
            var processes = Process.GetProcessesByName(procName).Where(p => p.MainModule.FileName == procPath);
            return processes.Count();
        }

        private Task<bool> StopProcessAsync(string procName)
        {
            var task = Task.Run(async () =>
              {
                  try
                  {
                      KillProcess(procName);

                      int times = 5;
                      while (GetProcessCount(procName) > 0)
                      {
                          times--;
                          if (times < 0) return false;
                          await Task.Delay(3000);
                      }
                  }
                  catch (Exception ex)
                  {
                      throw ex;
                  }
                  return true;
              });

            return task;
        }

        private Task<bool> StopProcessAsync(string procName, string targetPath)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    //获取当前运行的进程数量
                    var count = GetProcessCount(procName, targetPath);

                    KillProcess(procName, targetPath);

                    int times = 5;

                    if (count > 0)
                    {
                        while (GetProcessCount(procName, targetPath) == count)
                        {
                            times--;
                            if (times < 0) return false;
                            await Task.Delay(3000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return true;
            });

            return task;
        }


        private void ItemProject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ItemProject.SelectedItem as ProjectViewModel;
            if (item != null)
                item.IsChecked = !item.IsChecked;
        }

        private void ItemProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ItemProject.SelectedItem as ProjectViewModel;
            if (item != null)
                item.IsChecked = !item.IsChecked;
        }

        private async void batStart_Click(object sender, RoutedEventArgs e)
        {
            var exeList = GetExeList(exeName);

            var result = await StartProcessAsync(exeName, exeList);
            if (result)
            {
                NotifyStart();
                ShowMessage($"批量启动{exeList.Count}进程成功");
            }
        }

        private async void batStop_Click(object sender, RoutedEventArgs e)
        {
            var result = await StopProcessAsync(exeName);
            if (result)
            {
                NotifyStop();
                ShowMessage("批量停掉Exe成功");
            }
        }

        private List<string> GetExeList(string exeName)
        {
            List<string> exeList = new List<string>();

            foreach (var item in projectlist)
            {
                var root = GetTargetRoot(item);
                var targetExePath = System.IO.Path.Combine(root, item.Name, $"{exeName}.exe");

                if (File.Exists(targetExePath))
                {
                    item.IsEnable = true;

                    exeList.Add(targetExePath);
                }
            }
            return exeList;
        }

        private string GetTargetRoot(ProjectViewModel item)
        {
            var root = "";
            if (item.TypeId == 1)
            {
                root = yanchangRoot;
            }
            if (item.TypeId == 2)
            {
                root = ansenRoot;
            }
            return root;
        }

        private async void btnControl_Click(object sender, RoutedEventArgs e)
        {
            var item = ItemProject.SelectedItem as ProjectViewModel;

            var root = GetTargetRoot(item);
            var targetExePath = System.IO.Path.Combine(root, item.Name, $"{exeName}.exe");

            if (File.Exists(targetExePath))
            {
                item.IsEnable = true;
            }

            if (item.IsEnable)
            {
                if (item.IsStarted)
                {
                    var result = await StopProcessAsync(exeName, targetExePath);
                    if (result)
                    {
                        item.IsStarted = false;
                        ShowMessage($"停止{item.Name}成功");
                    }
                }
                else
                {
                    var result = await StartProcessAsync(exeName, targetExePath);
                    if (result)
                    {
                        item.IsStarted = true;
                        ShowMessage($"启动{item.Name}成功");
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetDll = $"{exeName}.dll";
            string sourceDllPath = System.IO.Path.Combine(executablePathRoot, targetDll);

            var item = ItemProject.SelectedItem as ProjectViewModel;
            var root = GetTargetRoot(item);

            var targetExePath = System.IO.Path.Combine(root, item.Name, $"{exeName}.exe");

            if (File.Exists(targetExePath))
            {
                item.IsEnable = true;
            }

            if (item.IsEnable)
            {
                try
                {
                    //停掉exe进程
                    var t1 = StopProcessAsync(exeName, targetExePath);

                    var t2 = t1.ContinueWith(t =>
                    {
                        if (t.Result)
                        {
                            NotifyStop(item);
                            //执行核心工作
                            return Excute(sourceDllPath, targetDll, item);
                        }
                        else
                        {
                            return false;
                        }
                    });

                    var t3 = t2.ContinueWith(async t =>
                    {
                        var result = await StartProcessAsync(exeName, targetExePath);
                        if (result)
                        {
                            NotifyStart();

                            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                 {
                                     ShowMessage($"{item.Name}更新成功");
                                 });
                        }
                    });
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message, null);
                }
            }
        }

        private void cbxAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in projectlist)
            {
                item.IsChecked = this.cbxAll.IsChecked.Value;
            }
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
        /// <summary>
        /// 全部文件更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void batUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetFolder = "Update";
            string sourceFolder = System.IO.Path.Combine(executablePathRoot, targetFolder);

            var exeList = GetExeList(exeName);
            try
            {
                //停掉exe进程
                var t1 = StopProcessAsync(exeName);

                var t2 = t1.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        NotifyStop();
                        //执行核心工作
                        return ExcuteBat(sourceFolder);
                    }
                    else
                    {
                        return false;
                    }
                });

                var t3 = t2.ContinueWith(async t =>
                {
                    var result = await StartProcessAsync(exeName, exeList);
                    if (result)
                    {
                        NotifyStart();

                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                            {
                                ShowMessage($"批量更新{exeList.Count}个成功");
                            });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, null);
            }
        }

        private async void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            string from = "540887384@qq.com";
            string fromer = "wbq3311";
            string to = "540887384@qq.com";
            string toer = "wbq";
            string Subject = "overtcp";
            string root = @"D:\Desktop\publish";

            List<string> files = new List<string>();
            files.Add(System.IO.Path.Combine(root, "overtcp.zip"));

            string Body = "ycbz";
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
                Thread.Sleep(200);
            }

            if (flag)

                ShowMessage($"邮件发送完毕，共有{files.Count}个邮件,成功{s}个，失败{f}个");

        }
        public async Task<bool> Sendmail(string sfrom, string sfromer, string sto, string stoer, string sSubject, string sBody, string sfile, string sSMTPHost, string sSMTPuser, string sSMTPpass)
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

        private void btnUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            var item = ItemProject.SelectedItem as ProjectViewModel;
            var root = GetTargetRoot(item);


            var targetExePath = System.IO.Path.Combine(root, item.Name, $"{exeName}.exe");

            if (File.Exists(targetExePath))
            {
                item.IsEnable = true;
            }

            string targetFolder = "Update";
            string sourceFolder = System.IO.Path.Combine(executablePathRoot, targetFolder);

            if (!item.IsEnable) return;
            try
            {
                //停掉exe进程
                var t1 = StopProcessAsync(exeName, targetExePath);

                var t2 = t1.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        NotifyStop();
                        //执行核心工作
                        return ExcuteBat(sourceFolder, item);
                    }
                    else
                    {
                        return false;
                    }
                });

                var t3 = t2.ContinueWith(async t =>
                {
                    var result = await StartProcessAsync(exeName, targetExePath);
                    if (result)
                    {
                        NotifyStart();

                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                            {
                                ShowMessage($"{item.Name}更新成功");
                            });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, null);
            }
        }


        private void btnReceiveEmail_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                //每3秒，检查邮件
                Thread.Sleep(3000);
                string host = "pop.qq.com";
                string user = "540887384@qq.com";
                string pass = "vkyuhqrejvuobfji";   //qclhpkrldvdzbfib
                int port = 995;

                string root = @"D:\Desktop\download";

                EmailHelper email = new EmailHelper(user, pass, host, port, true, null);
                string error = "";
                var isSuccess = email.ValidateAccount(ref error);

                if (isSuccess)
                {
                    var count = email.GetEmailCount();

                    if (count > 0)
                    {
                        for (int i = 1; i <= count; i++)
                        {
                            //收取附件
                            var flag = email.DownAttachmentsById(root, i);

                            if (flag)
                            {
                                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                {
                                    ShowMessage($"收取文件成功,当前MessageId：{i}");
                                });
                                break;
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
    }

    public class EmailHelper
    {
        private string accout; //邮箱账户
        private string pass;//邮箱密码
        private string popServer; //pop服务地址
        private int popPort; //pop服务端口号（110）
        private bool isUseSSL;
        private string ServerDataDB;

        public EmailHelper(string _accout, string _pass, string _popServer, int _popPort, bool _isUseSSL, string _ServerDataDB)
        {
            this.accout = _accout;
            this.pass = _pass;
            this.popServer = _popServer;
            this.popPort = _popPort;
            this.isUseSSL = _isUseSSL;
            this.ServerDataDB = _ServerDataDB;
        }

        #region 验证邮箱是否登录成功
        public bool ValidateAccount(ref string error)
        {
            Pop3Client client = new Pop3Client();
            try
            {
                client.Connect(popServer, popPort, isUseSSL);
                client.Authenticate(accout, pass);
            }
            catch (InvalidLoginException ex)
            {
                error = "邮箱登录失败！";
                Console.WriteLine("0.1邮箱登录失败");
                return false;
            }
            catch (InvalidUseException ex)
            {
                error = "邮箱登录失败！";
                Console.WriteLine("0.2邮箱登录失败");
                return false;
            }
            catch (PopServerNotFoundException ex)
            {
                error = "服务器没有找到！";
                Console.WriteLine("0.3服务器没有找到");
                return false;
            }
            catch (PopServerException ex)
            {
                error = "请在邮箱开通POP3/SMTP！";
                Console.WriteLine("0.4请在邮箱开通POP3/SMTP！");
                return false;
            }
            catch (Exception ex)
            {
                error = "连接出现异常";
                Console.WriteLine("0.5连接出现异常");
                return false;
            }
            finally
            {
                client.Disconnect();
            }
            return true;
        }
        #endregion

        #region
        /// <summary>
        /// 获取邮件数量
        /// </summary>
        /// <returns></returns>
        public int GetEmailCount()
        {
            int messageCount = 0;
            using (Pop3Client client = new Pop3Client())
            {
                if (client.Connected)
                {
                    client.Disconnect();
                }
                client.Connect(popServer, popPort, isUseSSL);
                client.Authenticate(accout, pass, AuthenticationMethod.UsernameAndPassword);
                messageCount = client.GetMessageCount();
            }

            return messageCount;
        }
        #endregion

        #region 下载邮件附件
        /// <summary>
        /// 下载邮件附件
        /// </summary>
        /// <param name="path">下载路径</param>
        /// <param name="messageId">邮件编号</param>
        public bool DownAttachmentsById(string path, int messageId)
        {
            using (Pop3Client client = new Pop3Client())
            {
                try
                {
                    if (client.Connected)
                    {
                        client.Disconnect();
                    }
                    client.Connect(popServer, popPort, isUseSSL);
                    client.Authenticate(accout, pass);
                    Message message = client.GetMessage(messageId);
                    string senders = message.Headers.From.DisplayName;
                    string from = message.Headers.From.Address;
                    string subject = message.Headers.Subject;
                    DateTime Datesent = message.Headers.DateSent;


                    List<MessagePart> messageParts = message.FindAllAttachments();

                    if (messageParts.Count == 0) return false;

                    foreach (var item in messageParts)
                    {
                        if (item.IsAttachment)
                        {
                            if (item.FileName.Contains(".zip"))
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                File.WriteAllBytes(System.IO.Path.Combine(path, item.FileName), item.Body);
                                break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取附件出错：" + ex.Message);
                    return false;
                }
            }

            return true;
        }
        #endregion
    }


    public class ControlRequest
    {
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceTypeId { get; set; }
        public string GroupName { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { get; set; }
        public string CommandType { get; set; }
        public string CommandParameter { get; set; }
        public string SessionId { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool UseMockData { get; set; }
        public string MockData { get; set; }
        public int? LinkId { get; set; }
    }

    public class ControlRequestDeHui : ControlRequest
    {
        public long StationId { set; get; }
        public string StationName { set; get; }
    }
}