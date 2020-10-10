using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyAssistant
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

        public decimal MaxDisplacement { get; set; }
        public string ProducingHorizon { set; get; }           //产油层位 
        [Alias("commission_date")]
        public DateTime? CommissionDate { set; get; }         //投产日期        
        [Alias("motor_type")]
        public string MotorType { set; get; }                //电机型号          
        [Alias("rated_power")]
        public double RatedPower { set; get; }                  //额定功率        
        [Alias("pumping_unit_type")]
        public string PumpingUnitType { set; get; }             //抽油机型号     

        public long DepId { set; get; } //			末结点部门编号		
        [Alias("Video_Id")]
        public int VideoId { set; get; } //				视频监控id			
        [Alias("oil_calibration")]
        public decimal OilCalibration { set; get; } //			采油单耗标定	  
        public decimal UpperLoadLine { set; get; } //		克斯奇上载荷线				 
        public decimal LowerLoadLine { set; get; } // 克斯奇下载荷线			 

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

        public int? LinkId { set; get; } //连接标识
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
}
