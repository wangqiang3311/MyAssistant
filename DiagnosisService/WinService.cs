using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace DiagnosisService
{
    partial class WinService : ServiceBase
    {
        public WinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("服务开启");
        }

        protected override void OnStop()
        {
            Console.WriteLine("服务停止");
        }
    }
}
