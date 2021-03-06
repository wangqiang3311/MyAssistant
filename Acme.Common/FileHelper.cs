﻿using ServiceStack.Redis;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Acme.Common
{
    public static class FileHelper
    {
        /// <summary>
        /// 删除大于30天的文件
        /// </summary>
        /// <param name="baseDirPath"></param>
        /// <param name="days"></param>
        public static void DeleteOldFiles(string baseDirPath, int days = 30)
        {
            var baseDir = new DirectoryInfo(baseDirPath);
            var files = baseDir.GetFiles();

            foreach (var item in files)
            {
                var ts = DateTime.Now - item.LastWriteTime;
                if (ts.TotalDays > days)
                {
                    item.Delete();
                }
            }
        }

        /// <summary>
        /// 删除所有子文件夹
        /// </summary>
        /// <param name="baseDirPath"></param>
        /// <param name="days"></param>
        public static void DeleteAllSubDirectories(string baseDirPath)
        {
            var baseDir = new DirectoryInfo(baseDirPath);
            var subDirectories = baseDir.GetDirectories();

            foreach (var item in subDirectories)
            {
                item.Delete(true);
            }
        }

        /// <summary>
        /// 删除文件夹下所有的文件
        /// </summary>
        /// <param name="file"></param>
        public static void DeleteDir(string file)
        {
            //判断文件夹是否还存在
            if (Directory.Exists(file))
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                foreach (string f in Directory.GetFileSystemEntries(file))
                {
                    if (System.IO.File.Exists(f))
                    {
                        //去除文件的只读属性
                        System.IO.File.SetAttributes(f, System.IO.FileAttributes.Normal);
                        //如果有子文件删除文件
                        System.IO.File.Delete(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteDir(f);
                    }
                }
                //删除空文件夹
                Directory.Delete(file);
            }
        }

        public static bool IsIpAddress(string host)
        {
            return Regex.IsMatch(host, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        }

        public static RedisEndpoint GetRedisEndpoint(RedisEndpoint redisEndpoint)
        {
            if (redisEndpoint == null)
                return null;

            try
            {
                if (!IsIpAddress(redisEndpoint.Host))
                {
                    var ip = Dns.GetHostEntryAsync(redisEndpoint.Host).GetAwaiter().GetResult();
                    redisEndpoint.Host = ip.AddressList[0].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return redisEndpoint;
        }

        public static string GetIpAddress()
        {
            var machineName = Dns.GetHostName();   //获取本机名
            var ipAdd = Dns.GetHostAddresses(machineName).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            return ipAdd.ToString();
        }

        public static byte[] FileToBytes(string fileName)
        {
            // 打开文件
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            var bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream
            return bytes;
        }

        public static byte[] StreamToBytes(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        public static byte[] StreamToBytes(MemoryStream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 将 byte[] 转成 Stream
        /// </summary>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - -
        * Stream 和 文件之间的转换
        * - - - - - - - - - - - - - - - - - - - - - - - */
        /// <summary>
        /// 将 Stream 写入文件
        /// </summary>
        public static void StreamToFile(Stream stream, string fileName)
        {
            // 把 Stream 转换成 byte[]
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);

            // 把 byte[] 写入文件
            var fs = new FileStream(fileName, FileMode.Create);
            var bw = new BinaryWriter(fs);
            bw.Write(bytes);
            bw.Close();
            fs.Close();
        }

        /// <summary>
        /// 从文件读取 Stream
        /// </summary>
        public static Stream FileToStream(string fileName)
        {
            // 打开文件
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            var bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        public static string ComputeFileMd5(string fileName)
        {
            var hashMd5 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (!System.IO.File.Exists(fileName)) return hashMd5;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                //计算文件的MD5值
                var calculator = System.Security.Cryptography.MD5.Create();
                var buffer = calculator.ComputeHash(fs);
                calculator.Clear();
                //将字节数组转换成十六进制的字符串形式
                var stringBuilder = new StringBuilder();
                foreach (var t in buffer)
                {
                    stringBuilder.Append(t.ToString("x2"));
                }
                hashMd5 = stringBuilder.ToString();
            }//关闭文件流
            return hashMd5;
        }

        public static string ComputeStringMd5(string input)
        {

            //计算文件的MD5值
            var calculator = System.Security.Cryptography.MD5.Create();
            var buffer = calculator.ComputeHash(Encoding.UTF8.GetBytes(input));
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            var stringBuilder = new StringBuilder();
            foreach (var t in buffer)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            var hashMd5 = stringBuilder.ToString();
            //关闭文件流
            return hashMd5;
        }

        //ComputeMD5

        public static string ComputeMd5(byte[] fileStream)
        {
            //计算文件的MD5值
            var calculator = System.Security.Cryptography.MD5.Create();
            var buffer = calculator.ComputeHash(fileStream);
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            var stringBuilder = new StringBuilder();
            foreach (var t in buffer)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            var hashMd5 = stringBuilder.ToString();

            return hashMd5;
        }

        public static string ComputeMd5(Stream fileStream)
        {
            //计算文件的MD5值
            var calculator = System.Security.Cryptography.MD5.Create();
            var buffer = calculator.ComputeHash(StreamToBytes(fileStream));
            calculator.Clear();
            //将字节数组转换成十六进制的字符串形式
            var stringBuilder = new StringBuilder();
            foreach (var t in buffer)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            var hashMd5 = stringBuilder.ToString();

            return hashMd5;
        }
    }
}
