
using ServiceStack;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Acme.Common
{
    public class ReportTable
    {
        [ApiMember(Name = "ErrCode", Description = "错误代码(0为正常，-1为不正常)",
            ParameterType = "query", DataType = "integer", IsRequired = true)]
        public int ErrCode { get; set; }

        [ApiMember(Name = "ErrMsg", Description = "错误消息",
            ParameterType = "query", DataType = "string", IsRequired = true)]
        public string ErrMsg { get; set; }
    }

    public class Report
    {
        [PrimaryKey]
        [Index]
        public int Id { get; set; }

        [Index]
        public string ReportId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime DateTime { get; set; }

        //企业应用可见范围（人员），其中包括userid
        [Alias("allow_users")]
        public List<string> AllowUsers { get; set; }

        //企业应用可见范围（部门）
        [Alias("allow_users")]
        public List<string> AllowPartys { get; set; }

        //企业应用可见范围（标签）
        [Alias("allow_users")]
        public List<string> AllowTags { get; set; }

        //和页面关联Id
        [Index]
        public string PageId { get; set; }
        public string CacheMode { get; set; }
        public string UserName { get; set; }
    }

    [Alias("report_view")]
    public class ReportView : Report
    {
        public string Script { get; set; }
        public string Parameter { get; set; }
        public string Md5 { get; set; }

        public DateTime ReceiveTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }
    }

    [Alias("reportdetails")]
    public class ReportDetail
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Index]
        public string ReportId { get; set; }

        [Index]
        public string DetailId { get; set; }

        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class GetReportListResponse : ReportTable
    {
        public List<ReportView> PrivateReportList { get; set; }
        public List<ReportView> GlobalReportList { get; set; }
        public List<ReportView> PermanentReportList { get; set; }
    }

    public class GetReportDetailResponse : ReportTable
    {
        public ReportView Report { get; set; }
        public List<ReportDetail> ReportDetailList { get; set; }
    }
}
