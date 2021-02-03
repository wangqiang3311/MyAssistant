using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWeb
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {
        private readonly ILogger<MyController> _logger;
        private readonly IOptions<LearningOptions> _learningOptions;
        private readonly IOptionsSnapshot<LearningOptions> _learningOptionsSnapshot;
        private readonly IOptionsMonitor<LearningOptions> _learningOptionsMonitor;
        private readonly IConfiguration _configuration;

        public MyController(ILogger<MyController> logger,
        IOptions<LearningOptions> learningOptions,
        IOptionsSnapshot<LearningOptions> learningOptionsSnapshot,
        IOptionsMonitor<LearningOptions> learningOptionsMonitor,
        IConfiguration configuration
        )
        {
            _logger = logger;
            _learningOptions = learningOptions;
            _learningOptionsSnapshot = learningOptionsSnapshot;
            _learningOptionsMonitor = learningOptionsMonitor;
            _configuration = configuration;
            _learningOptionsMonitor.OnChange((options, value) =>
            {
                _logger.LogInformation($"OnChnage => {JsonConvert.SerializeObject(options)}");
            });
        }

        [HttpGet("{action}")]
        public ActionResult GetOptions()
        {
            var builder = new StringBuilder();
            builder.AppendLine("learningOptions:");
            builder.AppendLine(JsonConvert.SerializeObject(_learningOptions.Value));
            builder.AppendLine("learningOptionsSnapshot:");
            builder.AppendLine(JsonConvert.SerializeObject(_learningOptionsSnapshot.Value));
            builder.AppendLine("learningOptionsMonitor:");
            builder.AppendLine(JsonConvert.SerializeObject(_learningOptionsMonitor.CurrentValue));
            return Content(builder.ToString());
        }
    }

    [Serializable]
    public class LearningOptions
    {
        public decimal Years { get; set; }
        public List<string> Topic { get; set; }
        public List<SkillItem> Skill { get; set; }
    }

    [Serializable]
    public class SkillItem
    {
        public string Lang { get; set; }
        public decimal? Score { get; set; }
    }
}
