
using System.Collections.Generic;

namespace Acme.Common
{
    public static class ServerConfig
    {
        public static string AppId { get; set; }
        public static IList<string> WebHostHttpUrlList { get; set; }
        public static IList<string> WebHostHttpsUrlList { get; set; }
        public static string BaseUrl { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }

        public static string ProjectPath { get; set; }

        static ServerConfig()
        {
            var appSettings = new ServiceStack.Configuration.AppSettings();

            //use default value if no config exists
            AppId = appSettings.Get<string>("AppID");
            WebHostHttpUrlList  = appSettings.GetList("WebAPI.WebHostUrl.Http");
            WebHostHttpsUrlList = appSettings.GetList("WebAPI.WebHostUrl.https");
            BaseUrl = appSettings.Get<string>("WebAPI.BaseUrl", "");

            UserName = appSettings.Get<string>("WebAPI.UserName");
            Password = appSettings.Get<string>("WebAPI.Password");

            ProjectPath = appSettings.Get<string>("HostingEnvironment.ContentRootPath", "~");
        }
    }

    public static class WeiXinWorkConfig
    {
        public static string CorpId { get; set; }
        //public static string CorpSecret { get; set; }
        public static string Token { get; set; }
        public static string EncodingAesKey { get; set; }

        public static string ProviderSecret { get; set; }
        public static string AppSecret { get; set; }
        public static string ContactsApiSecret { get; set; }

        public static string SysLogAgentId { get; set; }
        public static string UserLogAgentId { get; set; }

        static WeiXinWorkConfig()
        {
            var appSettings = new ServiceStack.Configuration.AppSettings();

            //use default value if no config exists
            CorpId = appSettings.Get<string>("WeiXin.Work.CorpId");
            //CorpSecret = appSettings.Get<string>($"WeiXin.Work.{CorpId}.CorpSecret");
            Token = appSettings.Get<string>($"WeiXin.Work.{CorpId}.Token");
            EncodingAesKey = appSettings.Get<string>($"WeiXin.Work.{CorpId}.EncodingAESKey");

            ProviderSecret = appSettings.Get<string>($"WeiXin.Work.{CorpId}.ProviderSecret");
            AppSecret = appSettings.Get<string>($"WeiXin.Work.{CorpId}.AppSecret", "");
            ContactsApiSecret = appSettings.Get<string>($"WeiXin.Work.{CorpId}.ContactsApiSecret");
            SysLogAgentId = appSettings.Get<string>($"WeiXin.Work.{CorpId}.SysLogAgentId");
            UserLogAgentId = appSettings.Get<string>($"WeiXin.Work.{CorpId}.UserLogAgentId");
        }
    }

    public static class WeiXinMpConfig
    {
        public static string AppId { get; set; }
        public static string AppSecret { get; set; }
        public static string Token { get; set; }
        public static string EncodingAesKey { get; set; }

        static WeiXinMpConfig()
        {
            var appSettings = new ServiceStack.Configuration.AppSettings();
            AppId = appSettings.Get<string>($"WeiXin.MP.AppId");
            AppSecret = appSettings.Get<string>($"WeiXin.MP.{AppId}.AppSecret");
            Token = appSettings.Get<string>($"WeiXin.MP.{AppId}.Token");
            EncodingAesKey = appSettings.Get<string>($"WeiXin.MP.{AppId}.EncodingAESKey");
        }
    }
}
