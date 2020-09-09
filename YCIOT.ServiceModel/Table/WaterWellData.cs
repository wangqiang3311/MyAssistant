
/*/////////////////////////////////////////////////////////////////////////////////////////////////////
* 文件名：[水井数据表结构]

* 作者：〈西安九派数据科技有限公司〉

* 描述：〈提供水井数据表结构〉

* 修改人：〈武尚春〉

* 完成时间：2020-03-03

* 修改内容：
*     2020-03-03： 原始修改
* 
*///////////////////////////////////////////////////////////////////////////////////////////////////////

using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel.Table
{

    //水井部门生产数据统计
    [Alias("waterwell_dep_prod_data")]
    public class WaterWellDepProdData
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; }                    //编号
        [Alias("dep_id")]
        public long DepId { set; get; }              //部门编码

        [Alias("date_id")]
        public int DateId { set; get; }                //数据日期
        [Alias("station_count")]
        public int StationCount { set; get; }          //配水间数
        [Alias("well_count")]
        public int WellCount { set; get; }             //注水井数
        [Alias("stop_count")]
        public int StopCount { set; get; }             //停井数
        [Alias("abnormal_count")]
        public int AbnormalCount { set; get; }         //异常井数
        [Alias("setted_flow")]
        public float SettedFlow { set; get; }          //配注量
        [Alias("real_cumulative_flow")]
        public float RealCumulativeFlow { set; get; }  //实注量
    }


    //水井部门生产数据统计
    [Alias("view_waterwell_dep_prod_data")]
    public class ViewWaterWellDepProdData : WaterWellDepProdData
    {
        public long ParentId { set; get; } //父编码 
        public string DepName { set; get; } //部门名称 
        public string DepCode { set; get; } //部门层级编码 
    }

    //水井生产数据统计
    [Alias("waterwell_prod_data")]
    public class WaterWellProdData
    {
        [Index]
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; }                     //编号
        [Alias("well_id")]
        public int WellId { set; get; }                 //水井ID

        [Alias("date_id")]
        public int DateId { set; get; }                 //日数据日期
        [Alias("setted_flow")]
        public float SettedFlow { set; get; }           //配注量
        [Alias("real_cumulative_flow")]
        public float RealCumulativeFlow { set; get; }   //实注量
    }

    //水井日数据统计
    [Alias("view_waterwell_prod_data")]
    public class ViewWaterWellProdData : WaterWellProdData
    {
        [Alias("well_name")]
        public string WellName { set; get; }        //水井名
        public long ParentId { set; get; } //父编码 
        public string DepName { set; get; } //部门名称 
        public string DepCode { set; get; } //部门层级编码 
    }


}
