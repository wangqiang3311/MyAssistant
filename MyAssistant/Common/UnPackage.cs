using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyAssistant.Common
{
    public class UnPackage
    {
        public static string Unzip(string zipFile, string desFolder)
        {
            string result = "";
            try
            {
                //解压缩包
                if (!Directory.Exists(desFolder)) Directory.CreateDirectory(desFolder);
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, desFolder, Encoding.UTF8, true); //解压到desFolder

                DirectoryInfo d = new DirectoryInfo(desFolder);
                var files = d.GetFiles();
                if (files.Length == 0)
                {
                    //找子目录
                    var ds = d.GetDirectories();
                    if (ds.Length > 0)
                    {
                        files = ds[0].GetFiles();
                    }
                }
                if (files.Length == 0) result = "压缩文件为空";
            }
            catch (Exception ex)
            {
                result += "解压出错：" + ex.Message;
            }
            return result;
        }
    }
}
