using System;
using System.ServiceProcess;

namespace DiagnosisService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBase[] services = new ServiceBase[] { new WinService() };
            ServiceBase.Run(services);
        }
    }
}
