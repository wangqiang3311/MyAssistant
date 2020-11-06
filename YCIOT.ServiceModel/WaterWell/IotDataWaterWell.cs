using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.WaterWell
{
    [Alias("IOT_Data_WaterWell")]
    public class IotDataWaterWell
    {
        [Index]
        [AutoIncrement]
        public int Id { get; set; }
        [Index]
        public long WellId { get; set; } //水井ID   
        public double? SettedFlow { get; set; } //设定流量（配注量）

        public double? TrunkPressure { get; set; } // 汇管管压

        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量
        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计

        /// <summary>
        /// 油压
        /// </summary>
        public double? OilPressure { get; set; }

        /// <summary>
        /// 套压
        /// </summary>
        public double? SleevePressure { get; set; }

        [Index]
        public int? ValveStatus { get; set; } //阀门控制状态
        [Index]
        public int? ValveMode { get; set; } //阀门工作模式
        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }
}
