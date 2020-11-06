using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    //油井功图
    [Alias("IOT_Data_OilWell_IndicatorDiagram")]
    public class IotDataOilWellIndicatorDiagram
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; }
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? Displacement { get; set; } //位移 
        public double? Stroke { get; set; } //冲次

        [Index]
        public double? MaxLoad { get; set; }    //最大载荷
        [Index]
        public double? MinLoad { get; set; }    //最小载荷 
        [Index]
        public double? AvgLoad { get; set; }    //平均载荷 

        public double? Interval { get; set; }    //采样间隔
        public int? Count { get; set; }          //采样点数
        public List<double> D { get; set; }     //位移
        public List<double> L { get; set; }     //载荷

        public bool Mock { get; set; }          //是否是模拟数据

        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址

        [Index]
        public int? AlarmCode { get; set; }     //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }
}
