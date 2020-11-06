using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.WaterWell
{
    [Alias("IOT_Data_WaterStation")]
    public class IotDataWaterStation
    {
        [Index]
        [AutoIncrement]
        public long Id { get; set; }
        [Index]
        public long StationId { get; set; } //配水间Id
        [Index]
        public double? TrunkPressure { get; set; } //汇管压力
        /// <summary>
        /// 泵压
        /// </summary>
        public double? PumpPressure { get; set; }

        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }
}
