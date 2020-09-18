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
    public partial class ReceiveTools : Window
    {
        public static IAppSettings appSettings = new AppSettings();
        public static string yanchangRoot;
        public static string ansenRoot;
        public static string yanchangs;
        public static string ansens;

        public ReceiveTools()
        {
            InitializeComponent();

            #region  创建消息通知
            ShowMessage("接收工具已启动");

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

        private void btnReceiveEmail_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string targetFolder = "Update";
            var targetPath = System.IO.Path.Combine(executablePathRoot, targetFolder);
            DeleteDir(targetPath);

            var assembliesWithServices = new Assembly[1];
            assembliesWithServices[0] = typeof(AppHost).Assembly;
            var appHost = new AppHost("AppHost", assembliesWithServices);
            var unzipToDir = appSettings.Get<string>("UnzipToDir");

            bool hasReceived = false;

            Task.Run(() =>
            {
                while (true)
                {
                    if (hasReceived)
                    {
                        break;
                    }

                    //每分钟，检查邮件
                    Thread.Sleep(1000);
                    string host = "pop.qq.com";
                    string user = "540887384@qq.com";
                    string pass = "vkyuhqrejvuobfji";   //qclhpkrldvdzbfib
                    int port = 995;

                    //解压到Update文件夹下


                    EmailHelper email = new EmailHelper(user, pass, host, port, true, null);
                    string error = "";
                    var isSuccess = email.ValidateAccount(ref error);

                    if (isSuccess)
                    {
                        var count = email.GetEmailCount();

                        if (count > 0)
                        {
                            var zipFilePath = "";
                            bool isPartZip = false;

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
                                    else
                                    {
                                        if (isPartZip == false)
                                            isPartZip = true;
                                    }
                                    if (zipFilePath != "" && i == 1)
                                    {
                                        //解压到同名目录下
                                        if (isPartZip)
                                        {
                                            var sourceDir = targetPath;
                                            var desPath = System.IO.Path.Combine(targetFolder, "release");
                                            var zipfileName = System.IO.Path.GetFileName(zipFilePath);
                                            var searchPattern = System.IO.Path.GetFileNameWithoutExtension(zipFilePath) + "*";
                                            ZipPackage.ZIPDecompress(sourceDir, desPath, zipfileName, searchPattern);

                                            hasReceived = true;
                                        }
                                        else
                                        {
                                            ZipPackage.Unzip(zipFilePath,System.IO.Path.Combine(targetPath,unzipToDir));
                                            hasReceived = true;

                                            if (isDeploy)
                                            {
                                                //auto copy
                                                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                                {
                                                    UpdateManage w = new UpdateManage();
                                                    w.UpdateAll();
                                                });
                                                isDeploy = false;
                                            }
                                        }
                                    }
                                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                                    {
                                        ShowMessage($"收取文件成功,当前MessageId：{i}");
                                    });
                                }
                            }
                        }
                    }
                }
            });
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

        private void Window_Closed(object sender, EventArgs e)
        {
            ConsoleManager.Toggle();
        }

        /// <summary>
        /// 是否部署
        /// </summary>
        bool isDeploy = false;
        private void btnAutoDep_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();

            isDeploy = true;
            btnReceiveEmail_Click(null, null);
        }
    }
}