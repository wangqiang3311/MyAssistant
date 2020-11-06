using System;
using Acme.Common;
using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel
{
    [Alias("Log_IOT_Modbus_Poll")]
    public class LogIotModbusPoll
    {
        [AutoIncrement]
        public long Id { get; set; } //自动编号
        [Index]
        public string UserId { get; set; } //操作人
        [Index]
        public string GroupName { get; set; } //组名

        public string LinkMode { get; set; }  //TCP  or  RTU

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }
        [Index]
        public byte ModbusAddress { get; set; } //Modbus地址 
        [Index]
        public string Type { get; set; } //操作类型  
        public string Parameter { get; set; } //操作参数
        public int State { get; set; }   //操作结果状态
        public string Result { get; set; } //操作结果
        [Index]
        public DateTime DateTime { get; set; } //操作时间
    }
}
