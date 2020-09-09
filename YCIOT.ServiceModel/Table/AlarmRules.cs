using System;
using ServiceStack.DataAnnotations;

namespace YCIOT.ServiceModel.Table
{
    public class DecisionRuleTypeBase
    {
        [Alias("rule_type_id")]
        public int RuleTypeId { set; get; } //规则类别Id
        [Alias("rule_type_name")]
        public string RuleTypeName { set; get; } //规则类别名称
        [Alias("parameter_template")]
        public string ParameterTemplate { set; get; } //规则类别参数 
    }

    //判定规则类型表
    [Alias("decision_rule_type")]
    public class DecisionRuleType : DecisionRuleTypeBase
    {
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //序号 
    }

    public class DecisionRuleBase
    {
        [Alias("object_id")]
        public int ObjectId { set; get; } //告警对象ID
        [Alias("object_type")]
        public int ObjectType { set; get; } //告警对象类别
        [Alias("rule_type_id")]
        public string RuleTypeId { set; get; } //规则类别Id
        [Alias("rule_parameter")]
        public string RuleParameter { set; get; } //规则参数 
        [Alias("trigger_type")]
        public int TriggerType { set; get; } //触发类型
        [Alias("trigger_parameter")]
        public string TriggerParameter { set; get; } //触发参数 
        [Alias("enable")]
        public bool Enable { set; get; } //是否有效
    }

    //判定规则表
    [Alias("decision_rule")]
    public class DecisionRule : DecisionRuleBase
    {
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //序号 
    }

    public class AlarmLogBase
    {
        [Alias("rule_id")]
        public int RuleId { set; get; }  //规则Id
        [Alias("alarm_time")]
        public DateTime AlarmTime { set; get; }  //告警时间
        [Alias("alarm_code")]
        public int AlarmCode { set; get; }       //告警码 
        [Alias("alarm_msg")]
        public string AlarmMsg { set; get; } //告警信息
    }

    //告警日志表
    [Alias("log_alarm")]
    public class AlarmLog : AlarmLogBase
    {
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //序号 
    }

    public class AlarmCodeBase
    {
        [Alias("alarm_code")]
        public int AlarmCode { set; get; }      //告警代码
        [Alias("description")]
        public int Description { set; get; }    //告警描述
        [Alias("measure")]
        public string Measure { set; get; }     //建议处置措施
    }

    //告警处置建议措施表
    [Alias("alarm_code")]
    public class AlarmCode : AlarmCodeBase
    {
        [AutoIncrement]
        [Alias("id")]
        public int Id { set; get; } //序号 
    }

    [EnumAsInt]     // Enum Saved as int
    public enum Region
    {
        Africa = 1,
        Americas = 2,
        Asia = 3,
        Australasia = 4,
        Europe = 5,
    }
}
