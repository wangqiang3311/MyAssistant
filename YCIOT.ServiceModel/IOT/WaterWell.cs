using ServiceStack.DataAnnotations;
using System;

namespace YCIOT.ServiceModel.IOT
{
    //配水间信息 2019-10-26
    [Alias("IOT_WaterStation")]
    public class IotWaterStation
    {
        [Index]
        [AutoIncrement]
        public long Id { get; set; }
        [Index]
        public long StationId { get; set; }   //配水间Id
        public string StationName { get; set; } //配水间名称
        [Index]
        public long DepId { set; get; }  //部门Id

        [Index]
        public int WaterWellCount { get; set; } //注水井数量
    }


    [Alias("View_IOT_WaterStation_Device")]
    public class ViewIotWaterStationDevice
    {
        public long DepId { get; set; }   //部门Id
        public long StationId { get; set; }   //配水间Id
        public string StationName { get; set; }   //配水间Id
        public int DeviceClass { set; get; }//设备类型 （协议箱等）
        public int DeviceTypeId { set; get; }
        public string DeviceModel { set; get; }//设备型号（GL2000-1）
        public string DeviceFirmware { set; get; }  //设备固件版本（1.0）
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { set; get; }//Modbus地址
        public bool Enable { set; get; }    //是否启用
        public bool Mock { set; get; }
    }


    //水井信息 2019-10-26
    [Alias("IOT_WaterWell")]
    public class IotWaterWell
    {
        [Index]
        [AutoIncrement]
        public long Id { get; set; }
        [Index]
        public long WellId { get; set; } //水井ID
        public string WellName { get; set; } //水井名称
        [Index]
        public long DepId { set; get; }  //部门Id
        [Index]
        public long StationId { get; set; } //配水间ID
        [Index]
        public string CengWei { get; set; } //层位
        public string ZhuShuiCengDuan { get; set; } //注水层段
        public DateTime TouChanRiQi { get; set; } //投产日期
        public double PeiZhuLiang { get; set; } //配注量  (硬件设置)
        public string TouZhuanZhuFangShi { get; set; } //投转注方式
        public string ZhuShuiFangShi { get; set; } //注水方式
    }

    [Alias("View_IOT_WaterWell_Device")]
    public class ViewIotWaterWellDevice
    {
        public long DepId { get; set; }   //部门Id
        public long WellId { get; set; } //水井ID
        public string WellName { get; set; } //水井名
        public long StationId { get; set; } //配水间ID
        public string StationName { get; set; } //配水间ID
        public int DeviceClass { set; get; }//设备类型 （水表、功图）
        public int DeviceTypeId { set; get; }
        public string DeviceModel { set; get; }//设备型号（GL2000-1）
        public string DeviceFirmware { set; get; }  //设备固件版本（1.0）
        public int SlotId { set; get; } //连接到上位机的槽号
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { set; get; }//Modbus地址
        public bool Enable { set; get; }    //是否启用
        public bool Mock { set; get; }
    }

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
        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }

    [Alias("IOT_Data_WaterStation_Latest")]
    public class IotDataWaterStationLatest
    {
        public long Id { get; set; }
        [Index]
        [PrimaryKey]

        public long StationId { get; set; } //配水间Id
        [Index]
        public double? TrunkPressure { get; set; } //汇管压力
        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
    }

    [Alias("View_IOT_Data_WaterStation_Latest")]
    public class ViewIotDataWaterStationLatest
    {
        public long WaterWellCount { get; set; }
        public long Id { get; set; }
        [Index]
        [PrimaryKey]
        public int StationId { get; set; } //配水间Id
        [Index]
        public double? TrunkPressure { get; set; } //汇管压力
        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public string StationName { set; get; }//配水间名称
    }

    [Alias("View_IOT_Data_WaterStation")]
    public class ViewIotDataWaterStation
    {
        public long WaterWellCount { get; set; }
        public long Id { get; set; }
        [Index]
        [PrimaryKey]

        public int StationId { get; set; } //配水间Id

        [Index]
        public double? TrunkPressure { get; set; } //汇管压力
        [Index]
        public DateTime DateTime { get; set; } //采集时间
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public string StationName { set; get; }//配水间名称
    }

    //注水间模拟数据
    [Alias("IOT_Data_WaterStation_Mock")]
    public class IotDataWaterStationMock : IotDataWaterStationLatest
    {
    }

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

    [Alias("IOT_Data_WaterWell_Latest")]
    public class IotDataWaterWellLatest
    {
        public int Id { get; set; }
        [Index]
        [PrimaryKey]
        public long WellId { get; set; } //水井ID   
        public double? SettedFlow { get; set; } //设定流量（配注量）
        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量

        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计
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

    [Alias("View_IOT_Data_WaterWell_Latest")]
    public class ViewIotDataWaterWellLatest
    {
        public int Id { get; set; }
        [Index]
        [PrimaryKey]
        public long WellId { get; set; } //水井ID   
        public double? SettedFlow { get; set; } //设定流量（配注量）
        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量

        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计
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
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long StationId { set; get; }//配水间Id
        public string StationName { set; get; }//配水间名称
    }

    [Alias("View_IOT_Data_WaterWell")]
    public class ViewIotDataWaterWell
    {
        public int Id { get; set; }
        [Index]
        [PrimaryKey]
        public long WellId { get; set; } //水井ID   
        public double? SettedFlow { get; set; } //设定流量（配注量）
        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量

        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计
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
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long StationId { set; get; }//配水间Id
        public string StationName { set; get; }//配水间名称
    }

    //水井模拟数据
    [Alias("IOT_Data_WaterWell_Mock")]
    public class IotDataWaterWellMock : IotDataWaterWellLatest
    {
    }

    [Alias("View_WaterStation_WaterWell")]
    public class ViewWaterStationWaterWell
    {
        public int Id { get; set; }
        [Index]
        [PrimaryKey]
        public long WellId { get; set; } //水井ID   

        public double? SettedFlow { get; set; } //设定流量（配注量）
        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量
        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计
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
        public string WaterWellCount { get; set; }
        public string TrunkPressure { get; set; }
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long StationId { set; get; }//配水间Id
        public string StationName { set; get; }//配水间名称
    }

    [Alias("View_WaterStation_WaterWell_Latest")]
    public class ViewWaterStationWaterWellLatest
    {
        public int Id { get; set; }
        [Index]
        [PrimaryKey]
        public long WellId { get; set; } //水井ID   

        public double? SettedFlow { get; set; } //设定流量（配注量）
        [Index]
        public double? TubePressure { get; set; } //管压
        [Index]
        public double? InstantaneousFlow { get; set; } //瞬时流量
        [Index]
        public double? DayCumulativeFlow { get; set; } //当日累计
        [Index]
        public double? CumulativeFlow { get; set; } //表头累计
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
        public string WaterWellCount { get; set; }
        public string TrunkPressure { get; set; }
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long StationId { set; get; }//配水间Id
        public string StationName { set; get; }//配水间名称
    }

    public class WaterWellDepDayData
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //编号
        [Alias("dep_id")]
        public string DepId { set; get; } //部门编码
        [Alias("dep_name")]
        public string DepName { set; get; } //部门名称
        [Alias("date_id")]
        public int DateId { set; get; } //日数据日期
        [Alias("total_Station_number")]
        public int TotalStationNumber { set; get; } //配水间数
        [Alias("total_well_number")]
        public int TotalWellNumber { set; get; } //总注水井数
        [Alias("total_stop_well_number")]
        public int TotalStopWellNumber { set; get; } //停井数
        [Alias("total_abnormal_well_number")]
        public int TotalAbnormalWellNumber { set; get; } //异常井数
        [Alias("total_day_setted_flow")]
        public float TotalDaySettedFlow { set; get; } //当日总配注量
        [Alias("total_day_cumulative_flow")]
        public float TotalDayCumulativeFlow { set; get; } //当日总累计
        [Alias("coincidence_rate")]
        public float CoincidenceRate { set; get; } //配注符合率
    }


}