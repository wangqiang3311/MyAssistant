using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace YCIOT.ServiceModel.OilWell
{
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
        /// 动液面高度,默认600米
        /// </summary>
        public double MovefluidHeight { set; get; }

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
}
