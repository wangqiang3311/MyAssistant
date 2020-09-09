using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Acme.Common
{
    public class WebApiResponse
    {
        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }
    }

    public class ScreenShotResponse : WebApiResponse
    {
        public string SessionId { get; set; }
    }
}

