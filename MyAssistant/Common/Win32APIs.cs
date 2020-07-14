using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MNote 
{
    public static class Win32APIs
    {

        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);


        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();


        public static void MinimizeRelease()
        {
            IntPtr pHandle = GetCurrentProcess();
            SetProcessWorkingSetSize(pHandle, -1, -1);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 判断指定的窗口是否已经被最小化。
        /// </summary>
        /// <param name="hWnd">窗口句柄。</param>
        /// <returns>如果窗口时最小化，则返回非零，否则返回零。</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr IsIconic(IntPtr hWnd);

        /// <summary>
        /// 控制窗口的可见性。
        /// </summary>
        /// <param name="hWnd">窗口句柄。</param>
        /// <param name="nCmdShow">为窗口指定可视性方面的一个命令。</param>
        /// <returns>如窗口之前是可见的，则返回非零，否则返回零。</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, IntPtr nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);


        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);



        // Fields
        public const int MONITOR_DEFAULTTONEAREST = 2;
        public const int WM_GETMINMAXINFO = 0x24;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_NCLBUTTONDOWN = 0xa1;


        // Methods
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX monitorInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public enum HitTest
        {
            HTBORDER = 0x12,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 0x10,
            HTBOTTOMRIGHT = 0x11,
            HTCAPTION = 2,
            HTCLIENT = 1,
            HTCLOSE = 20,
            HTERROR = -2,
            HTGROWBOX = 4,
            HTHELP = 0x15,
            HTHSCROLL = 6,
            HTLEFT = 10,
            HTMAXBUTTON = 9,
            HTMENU = 5,
            HTMINBUTTON = 8,
            HTNOWHERE = 0,
            HTREDUCE = 8,
            HTRIGHT = 11,
            HTSIZE = 4,
            HTSYSMENU = 3,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTTRANSPARENT = -1,
            HTVSCROLL = 7,
            HTZOOM = 9
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public Win32APIs.POINT ptReserved;
            public Win32APIs.POINT ptMaxSize;
            public Win32APIs.POINT ptMaxPosition;
            public Win32APIs.POINT ptMinTrackSize;
            public Win32APIs.POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFOEX
        {
            public int cbSize;
            public Win32APIs.RECT rcMonitor;
            public Win32APIs.RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
