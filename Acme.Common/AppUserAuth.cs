using System;
using ServiceStack.Auth;

namespace Acme.Common
{
    public class AppUserAuth : UserAuth
    {
        public string WeiXinWorkUserId { get; set; }
        public string WeiXinMpUserId { get; set; }
        public string DingTalkUserId { get; set; }

        public string ProfileUrl { get; set; }
        public string LastLoginIp { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
