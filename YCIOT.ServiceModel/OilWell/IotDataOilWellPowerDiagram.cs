using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    //油井功率图
    [Alias("IOT_Data_OilWell_PowerDiagram")]
    public class IotDataOilWellPowerDiagram
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; }
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? Displacement { get; set; } //位移 
        [Index]
        public double? MaxPower { get; set; }    //最大功率
        [Index]
        public double? MinPower { get; set; }    //最小功率 
        [Index]
        public double? AvgPower { get; set; }    //平均功率 
        [Index]
        public double? Interval { get; set; }    //采样间隔
        public int? Count { get; set; }          //采样点数
        public List<double> D { get; set; }     //位移
        public List<double> P { get; set; }     //功率
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        [Index]
        public int AlarmCode { get; set; }     //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }
}
