using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using System;
using System.IO;

namespace MyTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var sourceDir = @"D:\项目集\MyAssistant\MyAssistant\bin\Debug\netcoreapp3.1\Update\";
            var targetPath = sourceDir + "\\Release";
            var zipfileName = "ycbz.zip";
            var searchPattern = "ycbz*";
            ZIPDecompress(sourceDir, targetPath, zipfileName, searchPattern);
        }
        [Test]
        public void Test2()
        {
            var baseUrl = GetUrl("redis://clientid:ERe@3_rit!@112.126.101.120:6379?db=1");

            Uri uri = new Uri("http://"+baseUrl);


        }
        private static string GetUrl(string client)
        {
            string redisPushclientUrl = "";
            var linkString = client.Split('@');
            if (linkString.Length == 3)
            {
                var clientip = linkString[2].Split('?');//10.30.2.77:6379?db=15
                if (clientip.Length == 2)
                {
                    redisPushclientUrl = clientip[0];
                }
            }

            return redisPushclientUrl;
        }

        public static void ZIPDecompress(string sourceDir, string targetPath, string zipfileName, string searchPattern)
        {
            #region 判断参数
            const string ENDPATHFLG = "\\";
            zipfileName = zipfileName.ToLower();
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            //添加标记
            if (sourceDir.EndsWith(ENDPATHFLG) == false)
            {
                sourceDir += ENDPATHFLG;
            }

            if (targetPath.EndsWith(ENDPATHFLG) == false)
            {
                targetPath += ENDPATHFLG;
            }
            if (!File.Exists(sourceDir + zipfileName))
            {
                throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", sourceDir));
            }
            #endregion

            //读取文件,并过滤("ycbz*")
            string[] fileNames = Directory.GetFiles(sourceDir, searchPattern);
            if (fileNames == null || fileNames.Length == 0)
            {
                throw new Exception("压缩文件异常");
            }

            //将所有文件读取出来,然后放到内存流中
            System.IO.MemoryStream mStream = new MemoryStream();
            //先读取指定文件
            byte[] tBytes = null;

            //循环读取所有文件
            string tfileName = null;
            foreach (var fileItem in fileNames)
            {
                tfileName = System.IO.Path.GetFileName(fileItem).ToLower();
                //排除zip指定文件,让其最后加载
                if (zipfileName == tfileName)
                {
                    continue;
                }
                tBytes = File.ReadAllBytes(fileItem);
                mStream.Write(tBytes, 0, tBytes.Length);
            }

            tBytes = File.ReadAllBytes(sourceDir + zipfileName);
            mStream.Write(tBytes, 0, tBytes.Length);

            //注意必须设置Pos=0,否则会报EOF异常
            mStream.Position = 0;

            UnzipFromStream(mStream, targetPath);

        }

        public static void UnzipFromStream(Stream zipStream, string outFolder)
        {
            using (var zipInputStream = new ZipInputStream(zipStream))
            {
                while (zipInputStream.GetNextEntry() is ZipEntry zipEntry)
                {
                    var entryFileName = zipEntry.Name;
                    // To remove the folder from the entry:
                    //var entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here
                    // to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // 4K is optimum
                    var buffer = new byte[4096];

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Skip directory entry
                    if (Path.GetFileName(fullZipToPath).Length == 0)
                    {
                        continue;
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking
                    // to a buffer the full size of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                    }
                }
            }
        }
    }
}