using Microsoft.Extensions.DependencyInjection;
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
using ServiceStack.Data;
using ServiceStack.Host;
using ServiceStack.OrmLite;
using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
using YCIOT.ServiceModel.OilWell;

namespace MyAssistant
{
    /// <summary>
    /// UpdateManage.xaml 的交互逻辑
    /// </summary>
    public partial class ExportTools : Window
    {
        public static IAppSettings appSettings = new AppSettings();
        public static string yanchangRoot;
        public static string ansenRoot;
        public static string yanchangs;
        public static string ansens;

        public ExportTools()
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

        private void btnTemplateExport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNPOIExport_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var filePath = System.IO.Path.Combine(executablePathRoot, "DHTable.xlsx");
            IWorkbook workbook = null;
            ISheet sheet = null;
            try
            {
                workbook = WorkbookFactory.Create(filePath);
                sheet = workbook.GetSheetAt(2);//获取第2个工作薄

                var tableName = "";
                var fieldName = "";
                var fieldType = "";

                Dictionary<string, string> fields = new Dictionary<string, string>();

                for (var j = 1; j < 62; j++)
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

                        fieldType = ConvertToDBType(fieldType);

                        fieldName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fieldName);

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

                        fieldType = ConvertToDBType(fieldType);

                        fieldName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fieldName);
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

        private string ConvertToDBType(string type)
        {
            var transeType = type;
            switch (type.ToLower())
            {
                case "string":
                    transeType = "varchar(100)";
                    break;

                case "double":
                    transeType = "double(13,3)";
                    break;

                case "datetime":
                    transeType = "date";
                    break;

                case "long":
                    transeType = "bigint";
                    break;
            }

            return transeType;
        }

        private void btnNPOIExportEntity_Click(object sender, RoutedEventArgs e)
        {
            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var filePath = System.IO.Path.Combine(executablePathRoot, "DHTable.xlsx");
            IWorkbook workbook = null;
            ISheet sheet = null;
            try
            {
                workbook = WorkbookFactory.Create(filePath);
                sheet = workbook.GetSheetAt(2);//获取第二个工作薄

                var tableName = "";
                var fieldName = "";
                var fieldType = "";
                //描述
                var fieldDes = "";

                Dictionary<string, string> fields = new Dictionary<string, string>();

                for (var j = 1; j < 62; j++)
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

                        value = GetCellValue(row.Cells[3]);
                        fieldDes = value.ToString();

                        fieldName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fieldName);

                        fields.Add(fieldName, fieldType + "," + fieldDes);
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
                            if (i == 2)
                            {
                                fieldDes = v.ToString();
                            }
                            i++;
                        }
                        fieldName = fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);

                        fields.Add(fieldName, fieldType + "," + fieldDes);
                    }
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"[Alias(\"{tableName}\")]");
                sb.AppendLine($"public class {tableName}");
                sb.AppendLine("{");

                foreach (var item in fields)
                {
                    var v = item.Value;
                    var vd = v.Split(',');

                    var aliasName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(item.Key);

                    if (item.Key == "Id")
                    {
                        sb.AppendLine("[Index]");
                        sb.AppendLine("[AutoIncrement]");
                        sb.AppendLine(" /// <summary>");
                        sb.AppendLine($" /// {vd[1]}");
                        sb.AppendLine(" /// </summary>");

                        sb.AppendLine("[Alias(\"" + aliasName + "\")]");

                        sb.AppendLine("public   " + vd[0] + "  " + item.Key + " { set; get; }");
                    }
                    else
                    {
                        sb.AppendLine(" /// <summary>");
                        sb.AppendLine($" /// {vd[1]}");
                        sb.AppendLine(" /// </summary>");
                        sb.AppendLine("[Alias(\"" + aliasName + "\")]");
                        sb.AppendLine("public   " + vd[0] + "  " + item.Key + " { set; get; }");
                    }
                }
                sb.AppendLine("}");

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
        /// <summary>
        /// 导出功图数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCSVExportData_Click(object sender, RoutedEventArgs e)
        {
            var connectionFactory = App.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            using var dbFac = connectionFactory.OpenDbConnection();

            var appSettings = new AppSettings();
            var redisCon = appSettings.GetString("TestRedis");

            using var redisClient = new RedisClient(redisCon);

            var executablePathRoot = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            string targetFolder = "data";

            string dataDirPath = System.IO.Path.Combine(executablePathRoot, targetFolder);

            //获取文件夹下所有文件

            var files = Directory.GetFiles(dataDirPath);

            foreach (var file in files)
            {
                var filePath = file;

                string fileName = System.IO.Path.GetFileName(file).Replace(".csv","");
                string wellName = "";

                var indicatorDiagram = new IotDataOilWellIndicatorDiagram()
                {
                    AlarmCode = 0,
                    AlarmMsg = "正常"
                };

                indicatorDiagram.Mock = true;

                indicatorDiagram.D = new List<double>();  //位移
                indicatorDiagram.L = new List<double>();  //载荷

                indicatorDiagram.DateTime = DateTime.Now;

                IotOilWell oilWell = null;

                //根据文件名获取井Id
                var fileInfos = fileName.Split('-');

                if (fileInfos.Length > 1)
                {
                    wellName = $"{fileInfos[0]}-{fileInfos[1]}井";
                }
                if(fileInfos.Length == 1)
                {
                    wellName = $"{fileInfos[0]}井";
                }

                if (wellName != "")
                {
                    oilWell = dbFac.Single<IotOilWell>(w => w.WellName == wellName);

                    if (oilWell != null)
                    {
                        indicatorDiagram.WellId = oilWell.WellId;
                    }
                }

                try
                {
                    var dt = OpenCSV(filePath);

                    foreach (var item in dt)
                    {
                        if (item.Length > 1)
                        {
                            if (!string.IsNullOrEmpty(item[0]))
                            {
                                indicatorDiagram.D.Add(Math.Round(Convert.ToDouble(item[0]), 2));
                                indicatorDiagram.L.Add(Math.Round(Convert.ToDouble(item[1]), 2));
                            }
                        }
                    }
                    if (indicatorDiagram.L.Count > 0)
                    {
                        var maxLoad = Math.Round(indicatorDiagram.L.Max(), 2);//最大载荷
                        var minLoad = Math.Round(indicatorDiagram.L.Min(), 2);//最小载荷
                        var avgLoad = Math.Round(indicatorDiagram.L.Average(), 2);//平均载荷

                        //间隔 = 60（秒）/ 冲次 / 采样点数

                        int count = indicatorDiagram.D.Count;

                        //冲次2.5到3之间随机

                        var n = new Random().Next(1, 5);

                        var stroke = 2.5 + n * 0.1;

                        var interval = Math.Round(60 / stroke / count, 2);

                        indicatorDiagram.Displacement = Math.Round(indicatorDiagram.D.Max(), 2);
                        indicatorDiagram.Stroke = stroke;
                        indicatorDiagram.Interval = interval;

                        indicatorDiagram.MaxLoad = maxLoad;
                        indicatorDiagram.MinLoad = minLoad;
                        indicatorDiagram.AvgLoad = avgLoad;
                        indicatorDiagram.Count = count;

                        indicatorDiagram.Id = indicatorDiagram.WellId;

                        if(indicatorDiagram.D.Last()!= indicatorDiagram.D[0]){
                            indicatorDiagram.D.Add(indicatorDiagram.D[0]);
                        }
                        if(indicatorDiagram.L.Last() != indicatorDiagram.L[0])
                        {
                            indicatorDiagram.L.Add(indicatorDiagram.L[0]);
                        }
                        Console.WriteLine($"当前井：{oilWell.WellName}-{oilWell.WellId}");
                        //写入数据库

                        if (indicatorDiagram.WellId > 0)
                        {
                            var recordMock = indicatorDiagram.ConvertTo<IotDataOilWellIndicatorDiagramMock>();

                            if (dbFac.Exists<IotDataOilWellIndicatorDiagramMock>(d => d.WellId == recordMock.WellId))
                            {
                                var aCount = dbFac.Update(recordMock);

                                if (aCount > 0)
                                {
                                    Console.WriteLine($"{oilWell.WellName}-{oilWell.WellId}功图数据更新成功");
                                }
                            }
                            else
                            {
                                var isSuccess = dbFac.Save(recordMock);

                                if (isSuccess)
                                {
                                    Console.WriteLine($"{oilWell.WellName}-{oilWell.WellId}功图数据保存成功");
                                }
                            }

                            redisClient.AddItemToList("YCIOT:IOT_Data_OilWell_IndicatorDiagram", indicatorDiagram.ToJson().IndentJson());

                            if (oilWell != null)
                            {
                                redisClient.Set($"Group:OilWell:{oilWell.WellName}-{oilWell.WellId}:IndicatorDiagram", indicatorDiagram);
                                redisClient.Set($"Single:OilWell:IndicatorDiagram:{oilWell.WellName}-{oilWell.WellId}", indicatorDiagram);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("导出功图数据出错" + ex.Message);
                }
            }
        }


        public static List<string[]> OpenCSV(string filePath)//从csv读取数据返回table
        {
            System.Text.Encoding encoding = GetType(filePath); //Encoding.ASCII;//
            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);

            System.IO.StreamReader sr = new System.IO.StreamReader(fs, encoding);

            //记录每次读取的一行记录
            string strLine = "";

            List<string[]> list = new List<string[]>();

            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                var aryLine = strLine.Split(',');
                list.Add(aryLine);
            }
            sr.Close();
            fs.Close();
            return list;
        }


        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
        /// <param name="FILE_NAME">文件路径</param>
        /// <returns>文件的编码类型</returns>

        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            System.IO.FileStream fs = new System.IO.FileStream(FILE_NAME, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
            System.Text.Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// 通过给定的文件流，判断文件的编码类型
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(System.IO.FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            System.Text.Encoding reVal = System.Text.Encoding.Default;

            System.IO.BinaryReader r = new System.IO.BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }


        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;  //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
    }
}