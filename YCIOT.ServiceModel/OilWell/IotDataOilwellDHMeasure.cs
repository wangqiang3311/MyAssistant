using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
    /// <summary>
    /// 德汇计量
    /// </summary>
    [Alias("iot_data_oilwell_dh_measure")]
    public class IotDataOilwellDHMeasure
    {
        [Index]
        [AutoIncrement]
        public long Id { get; set; }    //Id

        public long StationId { set; get; }

        public string StationName { set; get; }

        public long WellFieldId { set; get; }
        public string WellFieldName { set; get; }

        public long WellId { set; get; }
        public string WellName { set; get; }

        public DateTime DateTime { get; set; }   //数据采集时间

        public bool JLZD { get; set; }    //计量自动
        public bool QJZD { get; set; }    //切井自动
        public bool WXAN { get; set; }    //维修按钮
        public bool SBGYBH { get; set; }    //设备高压保护
        public bool QTBH { get; set; }    //气体保护
        public bool SDBH { get; set; }    //湿度保护
        public bool WDBH { get; set; }    //温度保护
        public bool PFJ { get; set; }    //排风机
        public bool JRQ { get; set; }    //加热器
        public float JYYL { get; set; }    //进液压力
        public float CYYL { get; set; }    //出液压力
        public float HJWD { get; set; }    //环境温度
        public float HJSD { get; set; }    //环境湿度
        public float HJND { get; set; }    //环境浓度
        public float HSWZ { get; set; }    //活塞位置
        public float SFFK { get; set; }    //上阀反馈
        public float XFFK { get; set; }    //下阀反馈
        public float WTF1FK { get; set; }    //五通阀1反馈
        public float WTF2FK { get; set; }    //五通阀2反馈
        public float WTF3FK { get; set; }    //五通阀3反馈
        public float PGWD { get; set; }    //管排温度
        public float FZHS { get; set; }    //分钟含水


        public float LJCY { get; set; }    //累计产液
        public float DCRJ { get; set; }    //单仓容积
        public float SSCY { get; set; }    //实时产液
        public float SSPJHS { get; set; }    //实时平均含水

        public float ZRHS { get; set; }    //昨日含水
        public float YLGX { get; set; }    //压力高限
        public float ZRCY { get; set; }    //昨日产液
        public float YLDX { get; set; }    //压力低限

        public int QYBH { get; set; }    //区域编号
        public int JLQLX { get; set; }    //计量器类型
        public float SFKZ { get; set; }    //上阀控制
        public float XFKZ { get; set; }    //下阀控制


        public int SSYJH { get; set; }    //实时油井号
        public int ZRYJH { get; set; }    //昨日油井号
        public int ZRJH { get; set; }    //昨日井号

        public int SSJH { get; set; }    //实时井号
        public int ZCJS { get; set; }    //正常井数
        public int SZXS { get; set; }    //设置小时
        public int SZFZ { get; set; }    //设置分钟
        public int YCYSSJ { get; set; }    //音叉延时时间
        public int ZTMFM { get; set; }    //状态码：阀门
        public int ZTMJSY { get; set; }    //状态码：井上液
        public int SZZDQJZQ { get; set; }    //设置自动切井周期


        public float WDDX { get; set; }    //温度低限
        public float WDGX { get; set; }    //温度高限
        public float NDDX { get; set; }    //浓度低限
        public float NDGX { get; set; }    //浓度高限
        public float ZXYW { get; set; }    //左限液位
        public float YXYW { get; set; }    //右限液位



        public float SDGX { get; set; }    //湿度高限
        public float SDDX { get; set; }    //湿度低限

        public bool Mock { get; set; }          //是否是模拟数据
        public int DeviceTypeId { get; set; }  //设备类型编号
        public string NetworkNode { get; set; }    //网络节点地址
        public int AlarmCode { get; set; }
        public string AlarmMsg { get; set; }

    }
}
