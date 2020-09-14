using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using ServiceStack;
using ServiceStack.Configuration;
using NGS.Templater;
using MyAssistant.Common;
using Acme.ServiceInterface.Download;
using Acme.Common;

namespace Acme.ServiceInterface.Report
{
     public static class ReportFiles
     {
          public static bool Report(string documentFile, dynamic reportData)
          {
               try
               {
                    try
                    {
                         var type = Type.GetType("cu, NGS.Templater");
                         var provider = new RSACryptoServiceProvider(4096);
                         provider.FromXmlString(
                             "<RSAKeyValue><Modulus>1GBJodxOCcX8ZKR1JU8EpW5wSxYvFHgI26PSsyPTdMg+0VhQxx+vBCQXhoNjWJj5a2kmOSnjrWYQZSO2jWN7suecnrxAmJgY3ZpYwgstcgme5MAjO6AN0ycngegRDAWRiClzeZVDOxB7mp/TFHoQCM3gf5FbwBJvsQqYl4aRQKhlW70p5b9nOpjeg6AfzH0gXjDpc1swkJytHtVELehndLuOtns3onKSzw7Ec58lXr/wgCrMPvl3jneIgjI95X1KflwPBykBM0FyOmvkT2A0BbFtDepTAD8c0Xv1U5qtVvYOdlCNYwuynr1HKrLUVMHA66PoiMYSnxOCvaU09Znx3j+K9C9XYeYCAZ1ZOo5wtMOAEmDasIIfLQ3tRs4P3NjQVd9rrB7cbsBtPm0PJPYi20ZO9mb319SmKwUq2p/e42FIzcBiIak35gbQrlYvuMMZowPoo6mn1KZGuTxAxssQybUbRgIazC8GAwzNKBkSyvZIwFGYdtC2WPW7looimUJAIqllYGegBF2qerL8jp79j3TE+G9Y7aZlSSzONT6HWiNIfH4aAnqhh0YzjFddyb9lYI2ocfmyadzaGSh37o91bMoiBZ1upbSIEPlm+N0hc2AkC4YANdt84WEHshnBZyEencexFgU7hWkKKjn2Mb2tIsNRT4S0MMUtA5m8wqryQl0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                         if (type != null)
                         {
                              type.GetField("a", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, provider);
                         }
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine(ex.Message + "," + ex.StackTrace);
                    }
                    try
                    {
                         using var doc = Configuration.Factory.Open(documentFile);
                         doc.Process(reportData);
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine("执行数据处理出错:" + ex.Message + "," + ex.StackTrace);
                    }

                    return true;
               }
               catch (Exception ex)
               {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return false;
               }
          }
          public static object PublishFile(string template, string report, dynamic dy, bool isDownload = true)
          {
               var result = new ReportTable
               {
                    ErrCode = 0,
                    ErrMsg = "ok"
               };
               try
               {
                    var appSettings = new AppSettings();
                    var contentRootPath = appSettings.Get("HostingEnvironment.ContentRootPath", HostContext.AppHost.MapProjectPath("~/"));
                    var templatePath = HostContext.AppHost.MapProjectPath($"{ Path.Combine(contentRootPath, "template")}");
                    var reportPath = HostContext.AppHost.MapProjectPath($"{ Path.Combine(contentRootPath, "report")}");

                    if (!Directory.Exists(templatePath))
                    {
                         Directory.CreateDirectory(templatePath);
                    }

                    if (!Directory.Exists(reportPath))
                    {
                         Directory.CreateDirectory(reportPath);
                    }

                    var templateFilePath = Path.Combine(templatePath, template);
                    var reportFilePath = Path.Combine(reportPath, report);

                    Console.WriteLine("templateFilePath:" + templateFilePath);
                    Console.WriteLine("reportFilePath:" + reportFilePath);

                    FileInfo templateFile = new FileInfo(templateFilePath);

                    //将现有文件复制到新文件，不允许覆盖现有文件
                    templateFile.CopyTo(reportFilePath);

                    if (Report(reportFilePath, dy))
                    {
                         result.FilePath = reportFilePath;
                         if (isDownload)
                         {
                              var obj = FileDownload.Download(report, "report");
                              return obj;
                         }
                    }
                    return result;
               }
               catch (Exception ex)
               {
                    Console.WriteLine("PublishFile报错:" + ex.Message);

                    return result;
               }
          }
     }
}
