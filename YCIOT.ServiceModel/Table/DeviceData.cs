/*/////////////////////////////////////////////////////////////////////////////////////////////////////
* 文件名：[设备数据表结构]

* 作者：〈西安九派数据科技有限公司〉

* 描述：〈提供设备数据表结构〉

* 修改人：〈武尚春〉

* 完成时间：2020-03-03

* 修改内容：
*     2020-03-03： 原始修改
* 
*///////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel.Table
{
    //设备数据质量统计
    [Alias("Iot_Data_Device_Quality")]
    public class IotDataDeviceQuality
    {
        [Index] [AutoIncrement] [Alias("id")] public int Id { set; get; } //序号 

        [Alias("well_id")] 
        public long WellId { set; get; } //油井编号 
        [Alias("well_name")]
        public long WellName { set; get; } //油井编号 
        [Alias("team_id")]
        public string TeamId { set; get; } //采油队编码 
        [Alias("team_name")]
        public string TeamName { set; get; } //采油队名称 

        [Alias("date_id")] 
        public long DateId { set; get; } //日期标识

        [Alias("manufacturer_id")] 
        public string ManufacturerId { set; get; } //设备厂家编码

        [Alias("manufacturer_name")]
        public string ManufacturerName { set; get; } //设备厂家名称

        [Alias("device_id")] 
        public long DeviceId { set; get; } //设备编号 

        [Alias("device_name")]
        public long DeviceName { set; get; } //设备编号

        [Alias("shall_count")] 
        public long ShallCount { set; get; } //应采数 
        [Alias("real_count")] 
        public long RealCount { set; get; } //实采数 
        [Alias("normal_count")] 
        public long NormalCount { set; get; } //正常数 
        [Alias("abnormal_count")] 
        public long AbnormalCount { set; get; } //异常数 
        [Alias("normal_rate")] 
        public double NormalRate { set; get; } //数据正常率
    }


    //设备状态监测
    [Alias("device_state_monitor")]
    public class DeviceStateMonitor
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //序号 
        [Alias("Device_Class")]
        public int DeviceClass { set; get; } //设备类别 
       
        [Alias("device_count")]
        public int DeviceCount { set; get; } //设备接入数量
        [Alias("stop_count")]
        public int StopCount { set; get; } //停井设备数量
        [Alias("normal_count")]
        public int NormalCount { set; get; } //设备正常数量
        [Alias("abnormal_count")]
        public int AbnormalCount { set; get; } //设备异常数量
        [Alias("normal_rate")]
        public double NormalRate { set; get; } //设备正常率 
        [Alias("update_time")]
        public DateTime UpdateTime { set; get; } //数据更新时间
    }

    //设备故障申请
    [Alias("device_fault_apply")]
    public class DeviceFaultApply
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public long Id { set; get; } //序号 
        [Alias("team_id")]
        public string TeamId { set; get; } //采油队编码 
        [Alias("team_name")]
        public string TeamName { set; get; } //采油队名称 
        [Alias("well_id")]
        public long WellId { set; get; } //油井编号 
        [Alias("well_name")]
        public string WellName { set; get; } //油井名称 
        [Alias("manufacturer_id")]
        public string ManufacturerId { set; get; } //设备厂家编码
        [Alias("manufacturer_name")]
        public string ManufacturerName { set; get; } //设备厂家 
        [Alias("device_id")]
        public long DeviceId { set; get; } //设备编号 
        [Alias("device_name")]
        public string DeviceName { set; get; } //设备名称 
        [Alias("applicantor_id")]
        public long ApplicantorId { set; get; } //申请人编号 
        [Alias("applicantor")]
        public string Applicantor { set; get; } //申请人 
        [Alias("application_date")]
        public DateTime ApplicationDate { set; get; } //申请时间 
        [Alias("fault_category_id")]
        public long FaultCategoryId { set; get; } //故障类别ID 
        [Alias("fault_category_name")]
        public string FaultCategoryName { set; get; } //故障类别 
        [Alias("fault_description")]
        public string FaultDescription { set; get; } //故障描述 
        [Alias("process_status")]
        public string ProcessStatus { set; get; } //处理状态 
        [Alias("process_opinion")]
        public string ProcessOpinion { set; get; } //处理意见 
        [Alias("processor_id")]
        public long ProcessorId { set; get; } //处理人编号 
        [Alias("processor")]
        public string Processor { set; get; } //处理人 
        [Alias("process_time")]
        public DateTime ProcessTime { set; get; } //处理时间 
    }

}
