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
        public void UpdateAll()
        {
            batUpdateAll_Click(null, null);
        }
        /// <summary>
        /// 全部文件更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void batUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetFolder = "Update/overtcp";
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

            string targetFolder = "Update/overtcp";
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

    }
}