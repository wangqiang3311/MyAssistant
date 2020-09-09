using System;
using Acme.Common;
using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel.IOT
{
    public enum DeviceClass
    {
        PowerMeter = 100,  //电表
        IndicatorDiagram = 101, //功图
        CurrentDiagram = 102,//电流图
        PowerDiagram = 103,//功率图
        OilPumpingMachineController = 104, //抽油机控制器

        WaterTrunkPressureMeter = 200,//注水间干线压力表
        WaterInjectingInstrument = 201 //注水仪
    };

    //油井设备种类信息（某厂商 某型号设备表）
    [Alias("IOT_Device_Type")]
    public class IotDeviceType
    {
        [Index]
        [AutoIncrement]
        public int Id { set; get; }   
        public string Name { set; get; }//设备名字（无线功图）
        [Index]
        public int DeviceClass { set; get; }//设备类型 （水表、功图）
        [Index]
        public string DeviceModel { set; get; }//设备型号（PMC3000）
        [Index]
        public string Firmware { set; get; }   //设备固件版本（1.0）
        [Index]
        public string Vendor { set; get; }    //设备制造商（西安贵隆数字化工程科技有限公司）
    }

    [Alias("IOT_OilWell_Device")]
    public class IotOilWellDevice
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WellId { set; get; } //关联油井表
        [Index]
        public long DeviceTypeId { set; get; } //关联 ITO_Device_Type表
        public int SlotId { set; get; } //连接到上位机的槽号
        [Index]
        public DateTime InstallDate { set; get; } //设备安装日期
        [Index]
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码

        public string LinkMode { get; set; }  //TCP  or  RTU

        public string NetworkNode { get; set; }

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }

        public byte ModbusAddress { set; get; }//Modbus地址
        [Index]
        public bool Enable { set; get; }    //是否启用

        public bool Mock { set; get; }    //是否在读取失败时采用模拟数据
    }

    [Alias("IOT_WaterStation_Device")]
    public class IotWaterStationDevice
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long StationId { set; get; } //关联站表
        [Index]
        public long DeviceTypeId { set; get; } //关联 ITO_Device_Type表
        public int SlotId { set; get; } //连接到上位机的槽号
        [Index]
        public DateTime InstallDate { set; get; } //设备安装日期
        [Index]
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码

        public string NetworkNode { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }
        public byte ModbusAddress { set; get; }//Modbus地址
        [Index]
        public bool Enable { set; get; }    //是否启用
        public bool Mock { set; get; }    //是否在读取失败时采用模拟数据
    }

    [Alias("IOT_WaterWell_Device")]
    public class IotWaterWellDevice
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long WaterWellId { set; get; } //关联水井表
        [Index]
        public long DeviceTypeId { set; get; } //关联 ITO_Device_Type表
        public int SlotId { set; get; } //连接到上位机的槽号
        [Index]
        public DateTime InstallDate { set; get; } //设备安装日期
        [Index]
        public string GroupName { get; set; } //组名(用于ModBus分组读取)
        public string Password { get; set; } //操作密码
        public string NetworkNode { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }
        public byte ModbusAddress { set; get; }//Modbus地址
        [Index]
        public bool Enable { set; get; }    //是否启用
        public bool Mock { set; get; }    //是否在读取失败时采用模拟数据
    }

    [Alias("Log_IOT_Control")]
    public class LogIotControl
    {
        [AutoIncrement]
        public long Id { get; set; } //自动编号
        [Index]
        public string UserId { get; set; } //操作人
        [Index]
        public string Type { get; set; } //操作类型  
        public string Parameter { get; set; } //操作参数
        public int State { get; set; }   //操作结果状态
        public string Result { get; set; } //操作结果
        [Index]
        public DateTime DateTime { get; set; } //操作时间
    }

    [Alias("Log_IOT_Modbus_Poll")]
    public class LogIotModbusPoll
    {
        [AutoIncrement]
        public long Id { get; set; } //自动编号
        [Index]
        public string UserId { get; set; } //操作人
        [Index]
        public string GroupName { get; set; } //组名

        public string LinkMode { get; set; }  //TCP  or  RTU

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }
        [Index]
        public byte ModbusAddress { get; set; } //Modbus地址 
        [Index]
        public string Type { get; set; } //操作类型  
        public string Parameter { get; set; } //操作参数
        public int State { get; set; }   //操作结果状态
        public string Result { get; set; } //操作结果
        [Index]
        public DateTime DateTime { get; set; } //操作时间
    }
}
