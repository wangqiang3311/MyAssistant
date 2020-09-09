using System;
using System.Diagnostics;

namespace Acme.Common
{
    public static class ProcessHelper
    {
        /// <summary>
        /// 根据“精确进程名”结束进程
        /// </summary>
        /// <param name="strProcName">精确进程名</param>
        public static void KillProc(string strProcName)
        {
            try
            {
                //精确进程名  用GetProcessesByName
                foreach (var p in Process.GetProcessesByName(strProcName))
                {
                    if (!p.CloseMainWindow())
                    {
                        p.Kill();
                    }
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 根据 模糊进程名 结束进程
        /// </summary>
        /// <param name="strProcName">模糊进程名</param>
        public static void KillProcA(string strProcName)
        {
            try
            {
                //模糊进程名  枚举
                //Process[] ps = Process.GetProcesses();  //进程集合
                foreach (var p in Process.GetProcesses())
                {
                    Console.WriteLine(p.ProcessName + p.Id);

                    if (p.ProcessName.IndexOf(strProcName, StringComparison.Ordinal) <= -1) continue;
                    
                    if (!p.CloseMainWindow())
                    {
                        p.Kill();
                    }
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 判断是否包含此字串的进程   模糊
        /// </summary>
        /// <param name="strProcName">进程字符串</param>
        /// <returns>是否包含</returns>
        public static bool SearchProcA(string strProcName)
        {
            try
            {
                //模糊进程名  枚举
                //Process[] ps = Process.GetProcesses();  //进程集合
                foreach (var p in Process.GetProcesses())
                {
                    Console.WriteLine(p.ProcessName + p.Id);

                    if (p.ProcessName.IndexOf(strProcName, StringComparison.Ordinal) > -1)  //第一个字符匹配的话为0，这与VB不同
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 判断是否存在进程  精确
        /// </summary>
        /// <param name="strProcName">精确进程名</param>
        /// <returns>是否包含</returns>
        public static bool SearchProc(string strProcName)
        {
            try
            {
                //精确进程名  用GetProcessesByName
                var ps = Process.GetProcessesByName(strProcName);
                return ps.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
