using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    [Alias("IOT_Data_OilWell_PowerMeter")]
    public class IotDataOilWellPowerMeter
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }     //数据采集时间
        public double? APhaseVoltage { set; get; }  //A相电压
        public double? BPhaseVoltage { set; get; }  //B相电压
        public double? CPhaseVoltage { set; get; }  //C相电压
        public double? APhaseCurrent { set; get; }  //A相电流
        public double? BPhaseCurrent { set; get; }  //B相电流
        public double? CPhaseCurrent { set; get; }  //C相电流
        public double? TotalActivePower { get; set; } // 总有功功率
        public double? TotalReactivePower { get; set; }// 总无功功率
        public double? TotalApparentPower { get; set; }// 总视在功率
        public double? TotalPowerFactor { get; set; }  // 总功率因素
        public double? TotalActiveEnergy { get; set; } // 当前总有功电能
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; } //
    }
}
