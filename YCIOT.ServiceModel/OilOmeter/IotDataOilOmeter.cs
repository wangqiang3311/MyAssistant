using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilOmeter
{
    /// <summary>
    /// 储油罐液位采集记录表
    /// </summary>
    [Alias("iot_data_oil_ometer")]
    public class IotDataOilOmeter
    {
        /// <summary>
        /// 序号
        /// </summary>
        [Alias("id")]
        public int Id { get; set; }
        /// <summary>
        ///  油罐编号
        /// </summary>
        [Alias("oil_ometer_id")]
        public long OilOmeterId { get; set; }
        /// <summary>
        /// 告警代码
        /// </summary>
        [Alias("AlarmCode")]
        public int AlarmCode { get; set; }
        /// <summary>
        /// 告警信息
        /// </summary>
        [Alias("AlarmMsg")]
        public string AlarmMsg { get; set; }
        /// <summary>
        /// 液位实际高度
        /// </summary>
        [Alias("current_elevation")]
        public double CurrentElevation { get; set; }
        /// <summary>
        /// 数据采集时间
        /// </summary>
        [Alias("datetime")]
        public DateTime DateTime { get; set; }
        /// <summary>
        /// 油罐采集设备编号
        /// </summary>
        [Alias("DeviceTypeId")]
        public int DeviceTypeId { get; set; }

        /// <summary>
        ///前置网络节点地址，用于网络状态监测
        /// </summary>
        [Alias("NetworkNode")]
        public string NetworkNode { get; set; }

        public bool Mock { set; get; }    //是否在读取失败时采用模拟数据
    }
}
