using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funq;
using Microsoft.AspNetCore.Hosting;
using NLog;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Text;

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
            var logger = LogManager.GetCurrentClassLogger();

            var appSettings = new AppSettings();
            var debug = appSettings.Get<bool>("WebAPI.DebugMode");

            Config.BufferSyncSerializers = true;
            Config.UseSameSiteCookies = true;
            Config.AddRedirectParamsToQueryString = true;
            Config.DefaultRedirectPath = "/metadata";
            Config.AllowSessionCookies = true;
            Config.AllowNonHttpOnlyCookies = true;

            Config.AllowFileExtensions.Add("json");
            Config.AllowFileExtensions.Add("rar");
            Config.AllowFileExtensions.Add("zip");

            JsConfig.IncludeNullValues = true;

            JsConfig.DateHandler = DateHandler.ISO8601DateTime;
            JsConfig.TimeSpanHandler = TimeSpanHandler.StandardFormat;

            JsConfig<Guid>.SerializeFn = guid => guid.ToString("D");
            JsConfig<TimeSpan>.SerializeFn = time =>
                (time.Ticks < 0 ? "-" : "") + time.ToString("hh':'mm':'ss'.'fffffff");

            var contentRootPath = appSettings.Get("HostingEnvironment.ContentRootPath", MapProjectPath("~/"));
            var webRootPath = appSettings.Get("HostingEnvironment.WebRootPath", MapProjectPath("~/wwwroot"));

            HostContext.AppHost.Config.WebHostPhysicalPath = webRootPath;

            //下行代码解决supervisor下运行wwwroot目录找不准的问题
            HostingEnvironment.ContentRootPath = contentRootPath;
            HostingEnvironment.WebRootPath = webRootPath;

            logger.Info($"HostingEnvironment.ContentRootPath = {contentRootPath}");
            logger.Info($"HostingEnvironment.WebRootPath     = {webRootPath}");

            //Plugins.Add(new RequestLogsFeature());

            SetConfig(new HostConfig
            {
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), HostingEnvironment.IsDevelopment()),
                AddMaxAgeForStaticMimeTypes = new Dictionary<string, TimeSpan> {
                    { "image/gif", TimeSpan.FromHours(1) },
                    { "image/png", TimeSpan.FromHours(1) },
                    { "image/jpeg", TimeSpan.FromHours(1) },
                },
                DefaultJsonpCacheExpiration = new TimeSpan(0, 20, 0),
                ForbiddenPaths = new List<string>(),
                ApiVersion = "1.0",
            });

            if (debug)
            {
                logger.Debug("Config.DebugMode = true");

                SetConfig(new HostConfig
                {
                    //disable the metadata page
                    DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), HostingEnvironment.IsDevelopment()),
                });
            }
            else
            {
                logger.Info("Config.DebugMode = false");

                SetConfig(new HostConfig
                {
                    DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), HostingEnvironment.IsDevelopment()),
                });
            }

            var originWhitelist = appSettings.GetList("WebAPI.AllowOriginWhitelist");
            if (originWhitelist.Count > 0)
            {
                Plugins.Add(new CorsFeature(
                   allowOriginWhitelist: originWhitelist,
                   allowCredentials: true,
                   allowedHeaders: "Access-Control-Allow-Origin, Origin, Content-Type, Authorization, X-ss-id, X-ss-pid, X-ss-opts, X-UAId, ss-tok, X-ss-tok, withCredentials"
                  ));
                logger.Info("AllowOriginWhitelist:" + originWhitelist.Join(";") + ";");
            }
            else
            {
                Plugins.Add(new CorsFeature(
                  allowedOrigins: "*",
                  allowCredentials: true,
                  allowedHeaders: "Access-Control-Allow-Origin, Origin, Content-Type, Authorization, X-ss-id, X-ss-pid, X-ss-opts, X-UAId, ss-tok, X-ss-tok, withCredentials"
                 ));
                logger.Info("AllowOriginWhitelist: *");
            }

            //https://github.com/ServiceStack/ServiceStack/wiki/JWT-AuthProvider
            if (!debug)
            {
                var feature = Plugins.FirstOrDefault(x => x is MetadataFeature);
                Plugins.RemoveAll(x => x is MetadataFeature);
            }
        }
    }
}
