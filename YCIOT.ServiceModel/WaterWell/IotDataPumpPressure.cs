using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.WaterWell
{

    /// <summary>
    /// 泵压
    /// </summary>
    [Alias("iot_data_pump_pressure")]
    public class IotDataPumpPressure
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }

        public long PumpId { set; get; }   //泵编号

        public DateTime DateTime { get; set; }     //数据采集时间
        /// <summary>
        ///泵压
        /// </summary>
        public double? PumpPressure { set; get; }

        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }

}
