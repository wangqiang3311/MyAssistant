/*/////////////////////////////////////////////////////////////////////////////////////////////////////
* 文件名：[油井数据表结构]

* 作者：〈西安九派数据科技有限公司〉

* 描述：〈提供油井数据表结构〉

* 修改人：〈武尚春〉

* 完成时间：2020-03-03

* 修改内容：
*     2020-03-03： 原始修改
* 
*///////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using ServiceStack.DataAnnotations;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ServiceModel.Table
{
    //油井监测最新数据 
    public class OilWellMonitorDataLatest
    {
        public long Id { set; get; }
        public long WellId { set; get; }                    // 油井编号
        public string WellName { set; get; }                 //井名
        public string OilWellState { set; get; }             //油井状态          
        public string ProductHorizon { set; get; }           //产油层位          
        public DateTime CommissionDate { set; get; }         //投产日期          
        public string MotorType { set; get; }                //电机型号          
        public int RatedPower { set; get; }                  //额定功率          
        public string PumpUnitType { set; get; }             //抽油机型号        
        public int ManufacturerId { set; get; }              //设备厂家编码      
        public string Manufacturer { set; get; }              //设备厂家          
        public double YesterdayWorkingHours { set; get; }     //昨日工作时长      
        public double YesterdayFluidOutput { set; get; }      //昨日产液          
        public double YesterdayOilOutput { set; get; }         //昨日产油          
        public double YesterdayMoistureContent { set; get; }  //昨日含水率        
        public double MonthWorkingHours { set; get; }         //本月工作时长      
        public double MonthProdFluid { set; get; }            //本月产液          
        public double MonthProdOil { set; get; }              //本月产油          
        public double CompositeWaterCut { set; get; }         //本月昨日综合含水率		
        public DateTime UpdateTime { set; get; }              //更新时间	
        public string IsAlarm { set; get; }                   //是否有告警	
        public IotDataOilWellIndicatorDiagramLatest IndicatorDiagram { get; set; }
        public IotDataOilWellCurrentDiagramLatest CurrentDiagram { get; set; }
        public IotDataOilWellPowerDiagramLatest PowerDiagram { get; set; }
        public IotDataOilWellPowerMeterLatest PowerMeter { get; set; }
        public IotDataOilWellControllerStateLatest ControllerState { get; set; }
    }

    //油井生产数据
    [Alias("oilwell_prod_data")]
    public class OilWellProdData
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; }                       
        [Alias("well_id")]
        public int WellId { set; get; }                    // 油井编号
 
        [Alias("date_id")]
        public int DateId { set; get; }                    //数据日期标识
        [Alias("fluid_output")]
        public double FluidOutput { set; get; }            //产液
        [Alias("oil_output")]
        public double OilOutput { set; get; }              //产油（吨）
        [Alias("moisture_content")]
        public double MoistureContent { set; get; }        //含水率
        [Alias("working_hours")]
        public double DayWorkingHours { set; get; }        //工作时长
        [Alias("power_consumption")]
        public int PowerConsumption { set; get; }          //日耗电量
        [Alias("fluid_consumption")]
        public double FluidConsumption { set; get; }       //产液单耗
        [Alias("oil_consumption")]
        public double OilConsumption { set; get; }         //吨油单耗
        [Alias("pump_efficiency")]
        public double PumpEfficiency { set; get; }         //泵效
    }

    //油井生产数据视图
    [Alias("view_oilwell_prod_data")]
    public class ViewOilWellProdData : OilWellProdData
    {
        public string WellName { set; get; }
        public long ParentId { set; get; }        //父编码 
        public long DepId { set; get; }           //部门编号 
        public string DepName { set; get; }       //部门名称 
        public string DepCode { set; get; }       //部门层级编码 
        public long WellFieldId { set; get; }     //井场编码 
        public string WellFieldName { set; get; } //井场名称

    }

    //油井部门生产数据
    [Alias("oilwell_dep_prod_data")]
    public class OilWellDepProdData
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; }                //序号
        [Alias("dep_id")]
        public long DepId { set; get; }            //部门编码

        [Alias("date_id")]
        public int DateId { set; get; }            //日数据日期标识
        [Alias("well_count")]
        public int WellCount { set; get; }         //总井数
        [Alias("open_count")]
        public int OpenCount { set; get; }         //开井数

        [Alias("normal_count")]
        public int NormalCount { set; get; }       //正常井数
        [Alias("abnormal_count")]
        public int AbnormalCount { set; get; }     //异常井数
        [Alias("total_fluid")]
        public double TotalFluid { set; get; }         //总产液
        [Alias("total_oil")]
        public double TotalOil { set; get; }           //总产油
        [Alias("moisture_content")]
        public double MoistureContent { set; get; }    //含水率
    }

    [Alias("view_oilwell_dep_prod_data")]
    public class ViewOilWellDepProdData: OilWellDepProdData
    {
        public long ParentId { set; get; } //父编码 
        public string DepName { set; get; } //部门名称 
        public string DepCode { set; get; } //部门层级编码 
    }

    [Alias("Iot_WellField")]
    public class IotWellField
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }                 //  序号
        [Alias("well_field_id")]
        public long WellFieldId { set; get; }        // 井场ID
        [Alias("well_field_name")]
        public string WellFieldName { set; get; }    //井场名称
        [Alias("dep_id")]
        public long DepId { set; get; }             // 部门编号
        public int WellCount { set; get; }          //总井数
    }

}
