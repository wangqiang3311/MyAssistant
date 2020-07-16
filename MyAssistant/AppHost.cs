using System;
using System.IO;
using Funq;
using ServiceStack;
using ServiceStack.Configuration;

// ReSharper disable once CheckNamespace
namespace MyAssistant
{
    public class AppHost : AppHostBase
    {
        public AppHost(string serviceName, params System.Reflection.Assembly[] assembliesWithServices) : base(serviceName, assembliesWithServices)
        {
            var liveSettings = "~/appsettings.txt".MapProjectPath();
            AppSettings = File.Exists(liveSettings)
                ? (IAppSettings)new TextFileSettings(liveSettings)
                : new AppSettings();
        }

        public override void Configure(Container container)
        {
            throw new NotImplementedException();
        }
    }
}
