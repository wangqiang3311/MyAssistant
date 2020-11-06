using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    /// <summary>
    /// 油井压力（废弃）
    /// </summary>
    [Alias("iot_data_oilwell_pressure")]
    public class IotDataOilWellPressure
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }     //数据采集时间
        /// <summary>
        /// 压力
        /// </summary>
        public double? Pressure { set; get; }

        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }
}
