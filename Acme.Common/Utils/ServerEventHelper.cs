using Acme.Common.Utils;
using ServiceStack;
using ServiceStack.Configuration;
using System;
using System.Net;

namespace Acme.Common.Utils
{

    class PostRawToUserName
    {
        public string From { get; set; }
        public string ToUserName { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Selector { get; set; }
    }
    public static class ServerEventHelper
    {
        public static bool SendSseMessage(string userName, string sessionId, int errCode, string message)
        {

            try
            {
                var appSettings = new AppSettings();
                var baseUrl = appSettings.Get<string>("WebApi.BaseUrl");
                var apiKey = appSettings.Get<string>("WebApi.ApiKey");

                var controlResponse = new ControlResponse
                {
                    SessionId = sessionId,
                    ErrCode = errCode,
                    ErrMsg = message
                };

                var request = new PostRawToUserName
                {
                    From = "ModbusPoll",
                    ToUserName = userName,
                    Channel = "ModbusPoll",
                    Message = controlResponse.ToJson(),
                    Selector = $"trigger.job-{sessionId}"
                };

                var client = new JsonServiceClient(baseUrl)
                {
                    BearerToken = apiKey
                };

                var authResponse = client.Post<HttpWebResponse>(request);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
