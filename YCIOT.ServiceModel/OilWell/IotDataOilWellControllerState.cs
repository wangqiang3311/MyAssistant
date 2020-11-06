using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    [Alias("IOT_Data_OilWell_ControllerState")]
    public class IotDataOilWellControllerState
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }     //数据采集时间
        public double? FrequencyConversionActivePower { set; get; }  // 耗电量
        public double? CurrentBalance { set; get; }  // 电流平衡度
        public double? Stroke { set; get; }          // 当前冲次
        public double? MaxUpCurrent { set; get; }    // 上行最大电流
        public double? MaxDownCurrent { set; get; }  // 下行最大电流
        public int? StartAndStopStatus { set; get; }  // 起停状态
        public double? Frequency { get; set; }       //  运行频率
        public int? RunState { get; set; }           //  运行状态
        public double? MaxDisplacement { get; set; }  //抽油机冲程 --
        public int? WorkAndChangeFrequencySwitch { get; set; } //工变频切换
        public int? LocalAndFarSwitch { get; set; }    //就地远程切换
        public int? ChangeFrequencyFarStartAndStopStatus { get; set; } //变频远程启停状态
        public double? TotalRunTime { get; set; }      //变频累积运行时间

        /// <summary>
        /// 变频累积上电时间
        /// </summary>
        public double? TotalPowerupTime { get; set; }

        /// <summary>
        /// 变频当前上电时间
        /// </summary>
        public double? CurrentPowerupTime { get; set; }

        /// <summary>
        /// 变频当前运行时间
        /// </summary>
        public double? CurrentRunTime { get; set; }
        /// <summary>
        /// 功率因素角度
        /// </summary>
        public double? PowerFactorAngle { get; set; }

        /// <summary>
        /// 点数采集地址
        /// </summary>
        public int? CountFetchAddress { get; set; }

        /// <summary>
        /// 井口压力1
        /// </summary>
        public double? WellHeadPressure1 { get; set; }

        /// <summary>
        /// 井口压力2
        /// </summary>
        public double? WellHeadPressure2 { get; set; }

        /// <summary>
        /// 井口压力3
        /// </summary>
        public double? WellHeadPressure3 { get; set; }


        public double? BusBarVoltage { get; set; }     //变频母线电压
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }            // 告警代码
        public string AlarmMsg { get; set; }          // 告警信息
    }
}
