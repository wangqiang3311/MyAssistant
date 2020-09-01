using System;
using System.Collections.Generic;
using System.Text;

namespace MyAssistant.Common
{
    public class ControlRequest
    {
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceTypeId { get; set; }
        public string GroupName { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { get; set; }
        public string CommandType { get; set; }
        public string CommandParameter { get; set; }
        public string SessionId { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool UseMockData { get; set; }
        public string MockData { get; set; }
        public int? LinkId { get; set; }
    }

    public class ControlRequestDeHui : ControlRequest
    {
        public long StationId { set; get; }
        public string StationName { set; get; }
    }
}
