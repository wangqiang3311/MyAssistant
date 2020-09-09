using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel.IOT
{
    //油井信息
    [Alias("IOT_OilWell")]
    public class IotOilWell
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; }
        public string WellName { set; get; }
        [Index]
        public long WellFieldId { set; get; }
    }

    [Alias("View_IOT_OilWell_Device")]
    public class ViewIotOilWellDevice
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; }

        //ITO_Device_Type
        public int DeviceTypeId { set; get; }//设备类型编号
        [Index]
        public int DeviceClass { set; get; }//设备类型 （功图、电表）
        public string DeviceModel { set; get; }//设备型号（GL2000-1）
        //public string DeviceFirmware { set; get; }  //设备固件版本（1.0）

        //ITO_Well_Device
        public int SlotId { set; get; } //连接到上位机的槽号
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { set; get; }//Modbus地址
        public bool Enable { set; get; }    //是否启用
        public bool Mock { set; get; }    //读取失败时是否使用模拟数据

        public string WellName { set; get; }

        //public long DepId { set; get; }//部门Id
        //public long DepCode { set; get; }//部门Code
        //public string DepName { set; get; }//部门名称
        //public long ParentId { set; get; }//上级部门Id
        //public long WellFieldId { set; get; }//井场Id
        //public string WellFieldName { set; get; }//井场名称
    }
    
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
        public string NetworkNode{ get; set; }    //网络节点地址

        [Index]
        public int? AlarmCode { get; set; }     //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }


    //油井功图最新一条
    [Alias("IOT_Data_OilWell_IndicatorDiagram_Latest")]
    public class IotDataOilWellIndicatorDiagramLatest
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
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
        public int AlarmCode { get; set; }     //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }

    //油井功图模拟数据
    [Alias("IOT_Data_OilWell_IndicatorDiagram_Mock")]
    public class IotDataOilWellIndicatorDiagramMock : IotDataOilWellIndicatorDiagramLatest
    {
    }

    //油井功图最新一条视图
    [Alias("View_IOT_Data_OilWell_IndicatorDiagram_Latest")]
    public class ViewIotDataOilWellIndicatorDiagramLatest
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
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
        public int AlarmCode { get; set; }   //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息

        //public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }

        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称

    }

    [Alias("View_IOT_Data_OilWell_IndicatorDiagram")]
    public class ViewIotDataOilWellIndicatorDiagram
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
        public long WellId { set; get; }

        [Index]
        public DateTime? DateTime { get; set; }   //数据采集时间
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
        public int AlarmCode { get; set; }   //功图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

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

    //油井电流图最新一条
    [Alias("IOT_Data_OilWell_CurrentDiagram_Latest")]
    public class IotDataOilWellCurrentDiagramLatest
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
        public long WellId { set; get; }
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? Displacement { get; set; } //位移 
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
        public int AlarmCode { get; set; }//电流图诊断结果类型
        public string AlarmMsg { get; set; }   //诊断信息
    }

    //油井电流图最新一条视图
    [Alias("View_IOT_Data_OilWell_CurrentDiagram_Latest")]
    public class ViewIotDataOilWellCurrentDiagramLatest
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
        public long WellId { set; get; }

        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? Displacement { get; set; } //位移 
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
        public int AlarmCode { get; set; }//电流图诊断结果类型
        public string AlarmMsg { get; set; }   //诊断信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井电流图最新一条视图
    [Alias("View_IOT_Data_OilWell_CurrentDiagram")]
    public class ViewIotDataOilWellCurrentDiagram
    {
        public long Id { set; get; }
        [Index]
        [PrimaryKey]
        public long WellId { set; get; }
 
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? Displacement { get; set; } //位移 
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
        public int AlarmCode { get; set; }      //电流图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }

        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井电流图模拟数据
    [Alias("IOT_Data_OilWell_CurrentDiagram_Mock")]
    public class IotDataOilWellCurrentDiagramMock : IotDataOilWellCurrentDiagramLatest
    {
    }

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

    //油井功率图最新一条
    [Alias("IOT_Data_OilWell_PowerDiagram_Latest")]
    public class IotDataOilWellPowerDiagramLatest
    {
        public long Id { set; get; }
        [PrimaryKey]
        [Index]
        public long WellId { set; get; }
        [Index]
        public DateTime DateTime { get; set; }   //数据采集时间
        public double Displacement { get; set; } //位移 
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
        public int AlarmCode { get; set; }      //电流图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
    }

    //油井功率图最新一条视图
    [Alias("View_IOT_Data_OilWell_PowerDiagram_Latest")]
    public class ViewIotDataOilWellPowerDiagramLatest
    {
        public long Id { set; get; }
        [PrimaryKey]
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
        public int AlarmCode { get; set; }      //电流图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }

        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井功率图视图
    [Alias("View_IOT_Data_OilWell_PowerDiagram")]
    public class ViewIotDataOilWellPowerDiagram
    {
        public long Id { set; get; }
        [PrimaryKey]
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
        public int AlarmCode { get; set; }      //电流图诊断结果类型
        public string AlarmMsg { get; set; }    //诊断信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }

        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井功率图模拟数据
    [Alias("IOT_Data_OilWell_PowerDiagram_Mock")]
    public class IotDataOilWellPowerDiagramMock : IotDataOilWellPowerDiagramLatest
    {
    }

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

     /// <summary>
     /// 液位罐计量
     /// </summary>
     [Alias("iot_data_oilwell_tankmeasure")]
     public class IotDataOilWellTankmeasure
     {
          [Index]
          [AutoIncrement]
          public long Id { set; get; }
          public long TankId { set; get; }
          public DateTime DateTime { get; set; }     //数据采集时间
          /// <summary>
          /// 液位
          /// </summary>
          public double? Liquidlevel { set; get; }
        
          public bool Mock { get; set; }          //是否是模拟数据
          public int DeviceTypeId { get; set; }  //设备类型编号
          public string NetworkNode { get; set; }    //网络节点地址
          public int AlarmCode { get; set; }
          public string AlarmMsg { get; set; } 
     }

     /// <summary>
     /// 油井压力
     /// </summary>
     [Alias("iot_data_oilwell_pressure")]
     public class IotDataOilWellPressure
     {
          [Index]
          [AutoIncrement]
          public long Id { set; get; }
          public long WellId { set; get; }
          public DateTime DateTime { get; set; }     //数据采集时间
          /// <summary>
          /// 压力
          /// </summary>
          public double? Pressure { set; get; }

          public bool Mock { get; set; }          //是否是模拟数据
          public int DeviceTypeId { get; set; }  //设备类型编号
          public string NetworkNode { get; set; }    //网络节点地址
          public int AlarmCode { get; set; }
          public string AlarmMsg { get; set; }
     }

     /// <summary>
     /// 动液面
     /// </summary>
     [Alias("iot_data_oilwell_movefluidlevel")]
     public class IotDataOilWellMovefluidlevel
     {
          [Index]
          [AutoIncrement]
          public long Id { set; get; }
          public long WellId { set; get; }
          public DateTime DateTime { get; set; }     //数据采集时间
          /// <summary>
          /// 采样周期
          /// </summary>
          public int Period { set; get; }

          /// <summary>
          /// 采集模式
          /// </summary>
          public int Model { set; get; }

          /// <summary>
          /// 采集点数
          /// </summary>
          public int Count { set; get; }

          /// <summary>
          /// 采集序列
          /// </summary>
          public List<double> Y { set; get; }

          public bool Mock { get; set; }          //是否是模拟数据
          public int DeviceTypeId { get; set; }  //设备类型编号
          public string NetworkNode { get; set; }    //网络节点地址
          public int AlarmCode { get; set; }
          public string AlarmMsg { get; set; }
     }

    


     [Alias("IOT_Data_OilWell_PowerMeter_Latest")]
    public class IotDataOilWellPowerMeterLatest
    {
        public long Id { set; get; }
        [PrimaryKey]
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }     //数据采集时间
        public double? APhaseVoltage { set; get; }  //A相电压
        public double? BPhaseVoltage { set; get; }  //B相电压
        public double? CPhaseVoltage { set; get; }  //C相电压
        public double? APhaseCurrent { set; get; }  //A相电流
        public double? BPhaseCurrent { set; get; }  //B相电流
        public double? CPhaseCurrent { set; get; }  //C相电流
        public double? TotalActivePower { get; set; }   // 总有功功率
        public double? TotalReactivePower { get; set; } // 总无功功率
        public double? TotalApparentPower { get; set; } // 总视在功率
        public double? TotalPowerFactor { get; set; }   // 总功率因素
        public double? TotalActiveEnergy { get; set; }  // 当前总有功电能
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }  
    }

    [Alias("View_IOT_Data_OilWell_PowerMeter_Latest")]
    public class ViewIotDataOilWellPowerMeterLatest
    {
        public long Id { set; get; }
        [PrimaryKey]

        public long WellId { set; get; }
        public DateTime DateTime { get; set; }        //数据采集时间
        public double? APhaseVoltage { set; get; }      //A相电压
        public double? BPhaseVoltage { set; get; }      //B相电压
        public double? CPhaseVoltage { set; get; }      //C相电压
        public double? APhaseCurrent { set; get; }      //A相电流
        public double? BPhaseCurrent { set; get; }      //B相电流
        public double? CPhaseCurrent { set; get; }      //C相电流
        public double? TotalActivePower { get; set; }   // 总有功功率
        public double? TotalReactivePower { get; set; }  // 总无功功率
        public double? TotalApparentPower { get; set; }  // 总视在功率
        public double? TotalPowerFactor { get; set; }    // 总功率因素
        public double? TotalActiveEnergy { get; set; }   // 当前总有功电能
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }              // 告警代码
        public string AlarmMsg { get; set; }            // 告警信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }

        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称

        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称  //
    }

    [Alias("View_IOT_Data_OilWell_PowerMeter")]
    public class ViewIotDataOilWellPowerMeter
    {
        public long Id { set; get; }
        [PrimaryKey]

        public long WellId { set; get; }
        public DateTime DateTime { get; set; }   //数据采集时间
        public double? APhaseVoltage { set; get; }  //A相电压
        public double? BPhaseVoltage { set; get; }  //B相电压
        public double? CPhaseVoltage { set; get; }  //C相电压
        public double? APhaseCurrent { set; get; }  //A相电流
        public double? BPhaseCurrent { set; get; }  //B相电流
        public double? CPhaseCurrent { set; get; }  //C相电流
        public double? TotalActivePower { get; set; } // 总有功功率
        public double? TotalReactivePower { get; set; }// 总无功功率
        public double? TotalApparentPower { get; set; }// 总视在功率
        public double? TotalPowerFactor { get; set; }// 总功率因素
        public double? TotalActiveEnergy { get; set; }// 当前总有功电能
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }        // 告警代码
        public string AlarmMsg { get; set; }      // 告警信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井电表模拟数据
    [Alias("IOT_Data_OilWell_PowerMeter_Mock")]
    public class IotDataOilWellPowerMeterMock : IotDataOilWellPowerMeterLatest
    {
    }

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
        public double? BusBarVoltage { get; set; }     //变频母线电压
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }            // 告警代码
        public string AlarmMsg { get; set; }          // 告警信息
    }

    [Alias("IOT_Data_OilWell_ControllerState_Latest")]
    public class IotDataOilWellControllerStateLatest
    {
        public long Id { set; get; }
        [PrimaryKey]
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }      //数据采集时间
        public double? FrequencyConversionActivePower { set; get; }  // 耗电量
        public double? CurrentBalance { set; get; }  // 电流平衡度
        public double? Stroke { set; get; }          // 当前冲次
        public double? MaxUpCurrent { set; get; }    // 上行最大电流
        public double? MaxDownCurrent { set; get; }  // 下行最大电流
        public int? StartAndStopStatus { set; get; }  // 起停状态
        public double? Frequency { get; set; }        //  运行频率
        public int? RunState { get; set; }            //  运行状态
        public double? MaxDisplacement { get; set; }  //抽油机冲程 --
        public int? WorkAndChangeFrequencySwitch { get; set; } //工变频切换
        public int? LocalAndFarSwitch { get; set; }//就地远程切换
        public int? ChangeFrequencyFarStartAndStopStatus { get; set; } //变频远程启停状态
        public double? TotalRunTime { get; set; }  //变频累积运行时间
        public double? BusBarVoltage { get; set; } //变频母线电压
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }        //告警代码
        public string AlarmMsg { get; set; }      //告警信息
    }

    [Alias("View_IOT_Data_OilWell_ControllerState_Latest")]
    public class ViewIotDataOilWellControllerStateLatest
    {
        public long Id { set; get; }
        [PrimaryKey]
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }       //数据采集时间
        public double? FrequencyConversionActivePower { set; get; }  // 耗电量
        public double? CurrentBalance { set; get; }   // 电流平衡度
        public double? Stroke { set; get; }           // 当前冲次
        public double? MaxUpCurrent { set; get; }     // 上行最大电流
        public double? MaxDownCurrent { set; get; }   // 下行最大电流
        public int? StartAndStopStatus { set; get; }  // 起停状态
        public double? Frequency { get; set; }        //  运行频率
        public int? RunState { get; set; }            //  运行状态
        public double? MaxDisplacement { get; set; }  //抽油机冲程
        public int? WorkAndChangeFrequencySwitch { get; set; } //工变频切换
        public int? LocalAndFarSwitch { get; set; }  //就地远程切换
        public int? ChangeFrequencyFarStartAndStopStatus { get; set; } //变频远程启停状态
        public double? TotalRunTime { get; set; }    //变频累积运行时间
        public double? BusBarVoltage { get; set; }   //变频母线电压
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }          // 告警代码
        public string AlarmMsg { get; set; }        // 告警信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    [Alias("View_IOT_Data_OilWell_ControllerState")]
    public class ViewIotDataOilWellControllerState
    {
        public long Id { set; get; }
        [PrimaryKey]
        public long WellId { set; get; }
        public DateTime DateTime { get; set; }     //数据采集时间
        public double? FrequencyConversionActivePower { set; get; }  // 耗电量
        public double? CurrentBalance { set; get; }  // 电流平衡度
        public double? Stroke { set; get; }          // 当前冲次
        public double? MaxUpCurrent { set; get; }    // 上行最大电流
        public double? MaxDownCurrent { set; get; }  // 下行最大电流
        public int? StartAndStopStatus { set; get; }  // 起停状态
        public double? Frequency { get; set; }        //  运行频率
        public int? RunState { get; set; }            //  运行状态
        public double? MaxDisplacement { get; set; }  //抽油机冲程 --
        public int? WorkAndChangeFrequencySwitch { get; set; } //工变频切换
        public int? LocalAndFarSwitch { get; set; }   //就地远程切换
        public int? ChangeFrequencyFarStartAndStopStatus { get; set; } //变频远程启停状态
        public double? TotalRunTime { get; set; }     //变频累积运行时间
        public double? BusBarVoltage { get; set; }    //变频母线电压
        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }           //告警代码
        public string AlarmMsg { get; set; }         //告警信息
        public string DeviceClass { set; get; }
        //public string DeviceTypeId { set; get; }
        public string Vendor { set; get; }
        public string WellName { set; get; }
        public long DepId { set; get; }//部门Id
        public long ParentId { set; get; }//上级部门Id
        public long DepCode { set; get; }//部门Code
        public string DepName { set; get; }//部门名称
        public long WellFieldId { set; get; }//井场Id
        public string WellFieldName { set; get; }//井场名称
    }

    //油井控制器状态模拟数据
    [Alias("IOT_Data_OilWell_ControllerState_Mock")]
    public class IotDataOilWellControllerStateMock : IotDataOilWellControllerStateLatest
    {
    }
}
