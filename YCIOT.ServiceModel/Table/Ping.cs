using ServiceStack.DataAnnotations;
using System;

namespace YCIOT.ServiceModel.Table
{
    /// <summary>
    /// 网络Ping
    /// </summary>
    [Alias("view_ping_network_device_log")]
    public class ViewPingNetworkDeviceLog
    {
        [Index]
        [AutoIncrement]
        public long Id { set; get; }
        public double AvgRoundTripTimeMs { set; get; } //float ping时平均往返时间
        public int BroadCast { set; get; } //tinyint 是否启用推送
        public string CronString { set; get; } //varchar
        public string DeviceAddress { set; get; } //varchar 访问网址
        public long DeviceId { set; get; } //bigint  设备编号
        public string DeviceIpAddress { set; get; } //varchar 网络地址
        public string DeviceName { set; get; } //varchar 设备名称（交换机）
        public string DeviceType { set; get; } //varchar 设备类型（用图标分开图标）
        public int Enabled { set; get; } //tinyint 是否启用
        public string Group { set; get; } //varchar 分组
        public string Location { set; get; } //varchar 安装位置
        public int MaxRoundTripTimeMs { set; get; } //int ping时最大往返时间
        public int MinRoundTripTimeMs { set; get; } //int ping时最小往返时间
        public string NetworkAddress { set; get; } //varchar 网络地址
        public long ParentDeviceId { set; get; } //bigint 分级（父设备ID）
        public int Reps { set; get; } //int 代表（？）
        public string Status { set; get; } //varchar 当前状态
        public int SuccessCount { set; get; } //int 成功数
        public int TestCount { set; get; } //int 测试数
        public DateTime TestDateTime { set; get; } //datetime    测试时间
        public int TimeoutMs { set; get; } //int 超时时长
        public int Ttl { set; get; } //int
        public string TxtData { set; get; } //varchar 数据信息
        public int TxtDataSize { set; get; } //int 数据类型
        public int UseIPv6 { set; get; } //tinyint 是否启用ipv6
        public int UsePingSystemType { set; get; } //tinyint 是否启用ping类型
    }
}
