using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    //油井电流图
    [Alias("IOT_Data_OilWell_CurrentDiagram")]
    public class IotDataOilWellCurrentDiagram
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; }
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double Displacement { get; set; } //位移 
        [Index]
        public double? MaxCurrent { get; set; }    //最大电流
        [Index]
        public double? MinCurrent { get; set; }    //最小电流 
        [Index]
        public double? AvgCurrent { get; set; }    //平均电流 
        [Index]
        public double? Interval { get; set; }    //采样间隔
        public int? Count { get; set; }          //采样点数
        public List<double> D { get; set; }     //位移
        public List<double> C { get; set; }     //电流
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        [Index]
        public int AlarmCode { get; set; }     //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }
}
