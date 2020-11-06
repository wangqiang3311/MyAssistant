using Acme.Common;
using Acme.Common.Utils;
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
    public partial class UnPackWater : Window
    {
        private const string QueudId = "YCIOT";

        public static ObservableCollection<WaterLogViewModel> WaterList = new ObservableCollection<WaterLogViewModel>();
        public static ObservableCollection<WaterLogViewModel> DataSource = new ObservableCollection<WaterLogViewModel>();
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UnPackWater()
        {
            InitializeComponent();

            #region  创建消息通知
            ShowMessage("解包工具已启动");
            this.lbxPackage.ItemsSource = WaterList;
            #endregion

            EventManager.RegisterClassHandler(typeof(ListBoxItem),
ListBoxItem.MouseLeftButtonDownEvent,
new RoutedEventHandler(this.OnMouseLeftButtonDown));

        }

        private void OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {

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

        private async void btnStartJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WaterList.Clear();
                // Grab the Scheduler instance from the Factory 
                IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

                // and start it off
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<UnPackageJob>()
                   .WithIdentity("job1", "group1")
                   .Build();

                job.JobDataMap.Put("dispatcher", this.Dispatcher);
                // Trigger the job to run now, and then repeat every 600 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(600)
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

        private void WriteToResult(List<string> contents, bool isClear = true)
        {
            if (isClear)
                txtResult.Document.Blocks.Clear();

            foreach (var item in contents)
            {
                Paragraph p = new Paragraph();
                Run r = new Run(item);
                r.FontSize = 14;
                p.Inlines.Add(r);
                txtResult.Document.Blocks.Add(p);
            }
        }

        private void WriteToResult(string content)
        {
            txtResult.Document.Blocks.Clear();
            Paragraph p = new Paragraph();
            Run r = new Run(content);
            r.FontSize = 14;
            p.Inlines.Add(r);
            txtResult.Document.Blocks.Add(p);

        }

        public void GuoYi(string hex)
        {
            var s = HexStringToBytes(hex);
            var results = GuoYiWaterInject(s);
            if (results.Count > 0)
                WriteToResult(results, false);
        }
        public void AnSen(string hex)
        {
            var s = HexStringToBytes(hex);
            var results = AnSenWaterInject(s);
            if (results.Count > 0)
                WriteToResult(results, false);
        }

        public void JingHua(string hex)
        {
            var s = HexStringToBytes(hex);
            var results = JingHuaWaterInject(s);
            if (results.Count > 0)
                WriteToResult(results, false);
        }

        public List<string> JingHuaWaterInject(byte[] data)
        {
            List<string> results = new List<string>();

            if (data.Length < 8) return results;
            var client = new ModbusRtuOverTcp();


            var value = client.ByteTransform.TransInt16(data, 10);
            var settedFlow = value / 100.0; //设定流量回读


            value = client.ByteTransform.TransInt16(data, 4);

            var instantaneousFlow = value / 100.0; //瞬时流量

            var value3 = client.ByteTransform.TransInt16(data, 6);

            var value4 = client.ByteTransform.TransInt16(data, 8);

            var cumulativeFlow = (value3 << 16 + value4) / 100.0;//表头累计

            value = client.ByteTransform.TransInt16(data, 0);

            results.Add($"表头累计：{cumulativeFlow}, 瞬时流量：{instantaneousFlow}, 流量回读：{settedFlow}");

            var valveStatus = value; //阀门状态

            value = client.ByteTransform.TransInt16(data, 2);
            var valveMode = value; //阀门工作模式
            return results;
        }

        public List<string> GuoYiWaterInject(byte[] data)
        {
            List<string> results = new List<string>();

            if (data.Length < 8) return results;
            var client = new ModbusRtuOverTcp();

            var value8 = client.ByteTransform.TransUInt16(data, 8);
            var value6 = client.ByteTransform.TransUInt16(data, 6);

            var cumulativeFlow = (value6 << 16 | value8) / 100.0;

            var value = client.ByteTransform.TransInt16(data, 2);
            var settedFlow = value / 100.0; //设定流量回读

            value = client.ByteTransform.TransInt16(data, 14);
            var tubePressure = value / 100.0;//管压

            value = client.ByteTransform.TransInt16(data, 4);
            var instantaneousFlow = value / 100.0; //瞬时流量

            results.Add($"表头累计：{cumulativeFlow}, 瞬时流量：{instantaneousFlow}, 流量回读：{settedFlow}, 管压：{tubePressure}");

            value = client.ByteTransform.TransInt16(data, 0);
            var valveStatus = (value >> 4) & 0x01; //阀门状态
            var valveMode = (value >> 1) & 0x01; //阀门工作模式

            return results;
        }


        public List<string> AnSenWaterInject(byte[] data)
        {
            List<string> results = new List<string>();

            if (data.Length < 8) return results;
            var client = new ModbusRtuOverTcp();

            //配注仪状态: 0
            var r0 = data[0];
            var status = (BCDUtils.BCDToUshort(r0) >> 4) & 0x01;   //阀门状态
            var mode = (BCDUtils.BCDToUshort(r0) >> 1) & 0x01;    //阀门工作模式

            //设定流量回读: 0.17
            var t = BCDUtils.BCDToUshort((ushort)(data[2] << 8 | data[3]));

            var r1 = t / 100.0;

            //瞬时流量: 0.15
            t = BCDUtils.BCDToUshort((ushort)(data[4] << 8 | data[5]));
            var r2 = t / 100.0;


            //累计流量: 607.26
            var t1 = BCDUtils.BCDToUshort((ushort)(data[6] << 8 | data[7]));
            var t2 = BCDUtils.BCDToUshort((ushort)(data[8] << 8 | data[9]));

            var r3 = (t1 * 10000 + t2) / 100.0;

            //扩展: 0.0
            //水井压力: 7.35
            t = BCDUtils.BCDToUshort((ushort)(data[14] << 8 | data[15]));
            var r4 = t / 100.0;

            var actual = $"流量回读: {r1} ,管压: {r4} ,瞬时流量: {r2} ,表头累计: {r3}";
            results.Add(actual);
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

        private void btnUnpackGuoYi_Click(object sender, RoutedEventArgs e)
        {
            var text = this.txtUnpackageForWater.Text;

            //2020-09-28 11:37:40|Info|[M->1][39.144.5.151:24947]:01 03 14 00 04 00 2A 00 2A 00 02 AD 5A 00 00 00 00 00 00 00 00 00 00 8E A9

            var data = text;

            if (text.Contains(':'))
            {
                data = text.Split(':').Last();
            }
            data = data.Substring(9);
            GuoYi(data);
        }

        private void btnUnpackAnSen_Click(object sender, RoutedEventArgs e)
        {
            var text = this.txtUnpackageForWater.Text;

            //2020-09-28 11:37:40|Info|[M->1][39.144.5.151:24947]:01 03 14 00 04 00 2A 00 2A 00 02 AD 5A 00 00 00 00 00 00 00 00 00 00 8E A9

            var data = text;

            if (text.Contains(':'))
            {
                data = text.Split(':').Last();
            }
            data = data.Substring(9);
            AnSen(data);
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            List<WaterLogViewModel> dateList = new List<WaterLogViewModel>();
            List<WaterLogViewModel> linkIdList = new List<WaterLogViewModel>();

            List<WaterLogViewModel> source = DataSource.ToList();

            foreach (var item in WaterList)
            {
                if (!string.IsNullOrEmpty(this.txtDateTime.Text) && DateTime.Parse(item.DateCreateStr) > DateTime.Parse(this.txtDateTime.Text))
                {
                    dateList.Add(item);
                }
            }

            if (dateList.Count > 0) source = dateList;

            foreach (var item in source)
            {
                if (!string.IsNullOrEmpty(this.txtLinkId.Text) && item.From.Contains(this.txtLinkId.Text))
                {
                    linkIdList.Add(item);
                }
            }
            if (linkIdList.Count > 0) source = linkIdList;

            WaterList.Clear();

            foreach (var item in source)
            {
                WaterList.Add(item);
            }
            WriteToResult($"共{WaterList.Count}条数据");
        }

        private void btnUnpackGuoYiAuto_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in WaterList)
            {
                //2020-09-28 11:37:40|Info|[M->1][39.144.5.151:24947]:01 03 14 00 04 00 2A 00 2A 00 02 AD 5A 00 00 00 00 00 00 00 00 00 00 8E A9

                var data = item.To.Split(':').Last();

                data = data.Substring(9);

                GuoYi(data);
            }
        }

        private void btnLogAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //读取日志
            var dialog = new OpenFileDialog();
            dialog.Filter = ".txt|*.txt";
            if (dialog.ShowDialog(this) == false) return;
            var logPath = dialog.FileName;

            var lines = System.IO.File.ReadAllLines(logPath, Encoding.UTF8);

            Group(lines);
        }

        private void Group(string[] lines)
        {
            int m = 0;
            int n = 0;

            List<string> linkIds = new List<string>();

            int tcpCount = 0;

            foreach (var item in lines)
            {
                if (item.Contains("心跳包"))
                {
                    var match = Regex.Match(item, @"[\s\S]*\[(\d+)\]\[(\d+)\]心跳包");
                    if (match.Success)
                    {
                        var linkCount = int.Parse(match.Groups[1].Value);
                        tcpCount = tcpCount > linkCount ? tcpCount : linkCount;

                        var linkId = match.Groups[2].Value;
                        if (!linkIds.Contains(linkId))
                            linkIds.Add(linkId);
                    }
                }
                if (item.Contains("[1->M]"))
                {
                    m++;
                }
                if (item.Contains("[M->1]"))
                {
                    n++;
                }
            }

            WriteToResult($"最大共有{tcpCount}个tcp连接，{linkIds.Count}个linkId,[1->M]:{m}个,[M->1]:{n}个,有效率:{n * 1.0 / m}");
        }

        private void btnUnpackJingHua_Click(object sender, RoutedEventArgs e)
        {
            var text = this.txtUnpackageForWater.Text;

            //2020-09-28 11:37:40|Info|[M->1][39.144.5.151:24947]:01 03 14 00 04 00 2A 00 2A 00 02 AD 5A 00 00 00 00 00 00 00 00 00 00 8E A9

            var data = text;

            if (text.Contains(':'))
            {
                data = text.Split(':').Last();
            }
            data = data.Substring(9);
            JingHua(data);
        }
    }

    public class UnPackageJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                //读取日志
                var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                var logPath = System.IO.Path.Combine(executablePathRoot, "Log", "waterLog.txt");

                var lines = System.IO.File.ReadAllLines(logPath, Encoding.UTF8);

                var groups = Group(lines);

                var dataMap = context.MergedJobDataMap;
                var dispatcher = (Dispatcher)dataMap["dispatcher"];

                foreach (var item in groups)
                {
                    var i = 0;

                    dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                    {
                        var model = new WaterLogViewModel()
                        {
                            Description = item[i],
                            From = item[i + 1],
                            To = item[i + 2],
                            DateCreate = DateTime.Parse(item[i].Substring(0, 19))
                        };

                        UnPackWater.WaterList.Add(model);
                        UnPackWater.DataSource.Add(model);
                    });
                }
            });
        }
        private static List<List<string>> Group(string[] lines)
        {
            List<List<string>> linesByGroup = new List<List<string>>();

            int i = 0;
            int m = 0;
            int n = 0;
            int k = 0;


            List<string> infoLines = null;

            foreach (var item in lines)
            {
                i++;

                if (item.Contains("Get_XAGY_WII_WaterInjectingInstrument"))
                {
                    i = 0;
                    m = 0;
                    n = 0;
                    k = 0;
                    m++;
                    infoLines = new List<string>();
                    infoLines.Add(item);
                }
                if (item.Contains("[1->M]"))
                {
                    n++;
                    if (n - m < 2 && m > 0)
                    {
                        infoLines.Add(item);
                    }
                }
                if (item.Contains("[M->1]"))
                {
                    k++;
                    if (k - n < 3 && n > 0 && m > 0)
                    {
                        infoLines.Add(item);
                        linesByGroup.Add(infoLines);
                    }
                }
            }
            return linesByGroup;
        }
    }
}