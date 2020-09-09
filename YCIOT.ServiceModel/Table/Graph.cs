using ServiceStack.DataAnnotations;
using System;

namespace YCIOT.ServiceModel.Table
{
    /// <summary>
    /// 位置图
    /// </summary>
    [Alias("graph")]
    public class Graph
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long FigureId { set; get; }//图id
        public string Name { set; get; }  //图名 
        public string Description { set; get; }  //描述
        public DateTime CreateTime { set; get; }  //创建时间
    }

    /// <summary>
    /// 位置图明细
    /// </summary>
    [Alias("graph_detail")]
    public class GraphDetail
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        [Index]
        public long SpotId { set; get; }//点Id	
        [Index]
        public long GraphId { set; get; }//图id
        public string SpotName { set; get; }  //点名称 
        public string SpotType { set; get; }  //点类型 1工作站；2注水站；3井场；4配水间；5油井；6水井
        public decimal CoordX { set; get; }  //x位置
        public decimal CoordY { set; get; }  //y位置

    }

    //view_graph_detail
    /// <summary>
    /// 工作站、井场、配水间、注水站视图
    /// </summary>
    [Alias("view_graph_detail")]
    public class ViewGraphDetail
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        public decimal CoordX { set; get; }// decimalX
        public decimal CoordY { set; get; }//decimal Y
        public DateTime CreateTime { set; get; }//datetime    数据创建时间 数据创建时间
        public string Description { set; get; }//varchar 描述 描述
        public long FigureId { set; get; }//bigint  图id
        public long GraphId { set; get; }//bigint 图Id
        public string Name { set; get; }//varchar 图名 图名
        public long SpotId { set; get; }//bigint 点Id
        public string SpotName { set; get; }//varchar 点名称
        public int SpotType { set; get; }//int 点类型 1工作站；2注水站；3井场；4配水间；5油井；6水井
    }
}

