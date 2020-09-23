using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyAssistant.Common
{
    public class FileOper
    {
        /// <summary>
        /// 拷贝文件夹到指定文件夹并更改文件夹名称
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="aimPath">目标文件夹+文件夹名称</param>
        public static void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加
                if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    aimPath += System.IO.Path.DirectorySeparatorChar;
                }
                if (!System.IO.Directory.Exists(aimPath))
                {
                    System.IO.Directory.CreateDirectory(aimPath);
                }
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);

                foreach (string file in fileList)
                {
                    if (System.IO.Directory.Exists(file))
                    {
                        CopyDir(file, aimPath + Path.GetFileName(file));
                    }
                    else
                    {
                       File.Copy(file, aimPath + Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("备份文件夹出错：" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
