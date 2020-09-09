using Acme.Common.Utils;
using ServiceStack;
using YCIOT.ServiceModel.IOT;

namespace YCIOT.ServiceModel.MQ
{
    //[ExcludeMetadata]
    [Route("/mq/MockControl")]
    public class MockControl : ControlRequest, IReturn<MockControlResponse>
    {
    }

    public class MockControlResponse
    {
        public string Result { get; set; }
    }

    [Route("/mq/send/MockControlControl")]
    public class SendMockControlRequest : ControlRequest, IReturn<MockControlResponse>
    {

    }
}
