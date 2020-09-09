using ServiceStack;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ServiceModel.MQ
{
    //[ExcludeMetadata]
    [Route("/mq/DocxReport")]
    public class DocxReport : IReturn<DocxReportResponse>
    {
    }

    public class DocxReportResponse
    {
        public string Result { get; set; }
    }

    [Route("/mq/send/DocxReport")]
    public class DocxReportRequest : IReturn<DocxReportResponse>
    {

    }
}
