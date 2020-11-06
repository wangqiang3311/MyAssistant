using System;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Text;

namespace Acme.Common.Utils
{
    public static class DebugHelper
    {
        public static string MockRequest(string CommandType, long DeviceId = 1, int ModbusAddress = 1, string Name = "Device", int slot = -1, int linkId = 1)
        {
            var request = new ControlRequest()
            {
                CommandType = CommandType,
                RequestTime = DateTime.Now,
                DeviceId = DeviceId,
                ModbusAddress = ModbusAddress,
                DeviceName = Name,
                UseMockData = false,
                LinkId = linkId
            };

            var parDic = new Dictionary<long, double>();

            switch (CommandType)
            {
                #region  //浙江精华
                case "Get_ZJJH_WTPM_TrunkPressure": //读取配水间干压
                    request.CommandType = "Get_ZJJH_WTPM_TrunkPressure";
                    parDic.Add(1, 1);
                    break;

                case "Get_ZJJH_WII_WaterInjectingInstrument": //读取注水仪数据
                    request.CommandType = "Get_ZJJH_WII_WaterInjectingInstrument";
                    parDic.Add(1, 1);
                    break;

                case "Post_ZJJH_WII_InjectionAllocation": //下发配注量
                    request.CommandType = "Post_ZJJH_WII_InjectionAllocation";
                    parDic.Add(1, 1);
                    break;

                case "Post_ZJJH_WII_TubePressure": //下发管压设定
                    request.CommandType = "Post_ZJJH_WII_TubePressure";
                    parDic.Add(1, 1);
                    break;

                #endregion

                #region  //北京必创
                case "Get_BJBC_WTPM_TrunkPressure": //读取配水间干压
                    request.CommandType = "Get_BJBC_WTPM_TrunkPressure";
                    parDic.Add(1, 1);
                    break;

                case "Get_BJBC_WII_WaterInjectingInstrument": //读取注水仪数据
                    request.CommandType = "Get_BJBC_WII_WaterInjectingInstrument";
                    parDic.Add(1, 1);
                    break;

                case "Post_BJBC_WII_InjectionAllocation": //下发配注量
                    request.CommandType = "Post_BJBC_WII_InjectionAllocation";
                    parDic.Add(1, 1);
                    break;

                #endregion

                #region  //西安安森
                case "Get_XAAS_WTPM_TrunkPressure": //读取配水间干压
                    request.CommandType = "Get_XAAS_WTPM_TrunkPressure";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAAS_WII_WaterInjectingInstrument": //读取注水仪数据
                    request.CommandType = "Get_XAAS_WII_WaterInjectingInstrument";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAAS_WII_InjectionAllocation": //下发配注量
                    request.CommandType = "Post_XAAS_WII_InjectionAllocation";
                    parDic.Add(1, 1.44);
                    break;

                case "Post_XAAS_WII_TubePressure": //下发管压设定
                    request.CommandType = "Post_XAAS_WII_TubePressure";
                    parDic.Add(1, 1);
                    break;

                #endregion

                #region  //西安国仪
                case "Get_XAGY_WTPM_TrunkPressure": //读取配水间干压
                    request.CommandType = "Get_XAGY_WTPM_TrunkPressure";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGY_WII_WaterInjectingInstrument": //读取注水仪数据
                    request.CommandType = "Get_XAGY_WII_WaterInjectingInstrument";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGY_WII_InjectionAllocation": //下发配注量
                    request.CommandType = "Post_XAGY_WII_InjectionAllocation";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGY_WII_TubePressure": //下发管压设定
                    request.CommandType = "Post_XAGY_WII_TubePressure";
                    parDic.Add(1, 1);
                    break;

                #endregion

                #region 西安贵隆1000

                case "Get_XAGL_WM1000YXGT_IndicatorDiagram":  // 读取有线功图1000
                    request.CommandType = "Get_XAGL_WM1000YXGT_IndicatorDiagram";
                    parDic.Add(0, 1);
                    parDic.Add(1, 1);
                    break;

                #endregion

                #region  西安贵隆3000
                case "Get_XAGL_WM3000WXGT_IndicatorDiagram":  // 读取无线功图
                    request.CommandType = "Get_XAGL_WM3000WXGT_IndicatorDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000YXGT_IndicatorDiagram":  // 读取有线功图
                    request.CommandType = "Get_XAGL_WM3000YXGT_IndicatorDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000GLT_PowerDiagram":  // 读取功率图
                    request.CommandType = "Get_XAGL_WM3000GLT_PowerDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000DLT_CurrentDiagram":  // 读取电流图
                    request.CommandType = "Get_XAGL_WM3000DLT_CurrentDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000DB_PowerMeter":  // 读电参
                    request.CommandType = "Get_XAGL_WM3000DB_PowerMeter";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM3000KZG_StartWell":  // 开井操作
                    request.CommandType = "Post_XAGL_WM3000KZG_StartWell";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM3000KZG_StopWell":  // 关井操作
                    request.CommandType = "Post_XAGL_WM3000KZG_StopWell";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM3000KZG_StrokeFrequency":  // 下发冲次操作
                    request.CommandType = "Post_XAGL_WM3000KZG_StrokeFrequency";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000KZG_ControllerParameter":  // 读电机参数
                    request.CommandType = "Get_XAGL_WM3000KZG_ControllerParameter";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM3000KZG_ControllerStatus":  // 读变频器运行参数
                    request.CommandType = "Get_XAGL_WM3000KZG_ControllerStatus";
                    parDic.Add(1, 1);
                    break;
                #endregion

                #region 西安贵隆2000
                case "Get_XAGL_WM2000YXGT_IndicatorDiagram":  // 读取有线功图
                    request.CommandType = "Get_XAGL_WM2000YXGT_IndicatorDiagram";
                    parDic.Add(1, 1);
                    break;
                case "Get_XAGL_WM2000GLT_PowerDiagram":  // 读取功率图
                    request.CommandType = "Get_XAGL_WM2000GLT_PowerDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM2000DLT_CurrentDiagram":  // 读取电流图
                    request.CommandType = "Get_XAGL_WM2000DLT_CurrentDiagram";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM2000DB_PowerMeter":  // 读电参
                    request.CommandType = "Get_XAGL_WM2000DB_PowerMeter";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM2000KZG_StartWell":  // 开井操作
                    request.CommandType = "Post_XAGL_WM2000KZG_StartWell";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM2000KZG_StopWell":  // 关井操作
                    request.CommandType = "Post_XAGL_WM2000KZG_StopWell";
                    parDic.Add(1, 1);
                    break;

                case "Post_XAGL_WM2000KZG_StrokeFrequency":  // 下发冲次操作
                    request.CommandType = "Post_XAGL_WM2000KZG_StrokeFrequency";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM2000KZG_ControllerParameter":  // 读电机参数
                    request.CommandType = "Get_XAGL_WM2000KZG_ControllerParameter";
                    parDic.Add(1, 1);
                    break;

                case "Get_XAGL_WM2000KZG_ControllerStatus":  // 读变频器运行参数
                    request.CommandType = "Get_XAGL_WM2000KZG_ControllerStatus";
                    parDic.Add(1, 1);
                    break;
                #endregion

                #region//中科奥维
                case "Get_ZKAW_IndicatorDiagram_IndicatorDiagram":  // 读取无线功图
                    request.CommandType = "Get_ZKAW_IndicatorDiagram_IndicatorDiagram";
                    parDic.Add(1, 1);
                    break;
                #endregion

                #region  //洛阳乾禾
                case "Get_LYQH_WG_IndicatorDiagram":  // 读取有线功图
                    request.CommandType = "Get_LYQH_WG_IndicatorDiagram";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "POST_LYQH_GTShiShiCaiJI":  // 设置有线功图实时采集
                    request.CommandType = "POST_LYQH_GTShiShiCaiJI";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "POST_LYQH_DB_ShiShiCaiJI":  // 设置电参实时采集
                    request.CommandType = "POST_LYQH_DB_ShiShiCaiJI";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;


                case "POST_LYQH_WG_IndicatorDiagramSetting":  // 设置功图
                    request.CommandType = "POST_LYQH_WG_IndicatorDiagramSetting";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "POST_LYQH_DYMSetting":  // 设置动液面
                    request.CommandType = "POST_LYQH_DYMSetting";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "POST_LYQH_GJLSetting":  // 设置液位
                    request.CommandType = "POST_LYQH_GJLSetting";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;


                case "Get_LYQH_GLT_PowerDiagram":  // 读取功率图
                    request.CommandType = "Get_LYQH_GLT_PowerDiagram";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "Get_LYQH_DLT_CurrentDiagram":  // 读取电流图
                    request.CommandType = "Get_LYQH_DLT_CurrentDiagram";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "Get_LYQH_DB_PowerMeter":  // 读电参
                    request.CommandType = "Get_LYQH_DB_PowerMeter";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "Post_LYQH_KZG_StartWell":  // 开井操作
                    request.CommandType = "Post_LYQH_KZG_StartWell";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "POST_LYQH_KZG_StopWell":  // 关井操作
                    request.CommandType = "POST_LYQH_KZG_StopWell";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "Get_LYQH_KZG_StartWellTime":  //读取开井时间
                    request.CommandType = "Get_LYQH_KZG_StartWellTime";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;

                case "Get_LYQH_DYM_Movefluidlevel":
                    request.CommandType = "Get_LYQH_DYM_Movefluidlevel";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;
                case "POST_LYQH_DYMShiShiCaiJI":
                    request.CommandType = "POST_LYQH_DYMShiShiCaiJI";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;
                case "POST_LYQH_GJLShiShiCaiJI":
                    request.CommandType = "POST_LYQH_GJLShiShiCaiJI";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;
                case "Get_LYQH_GJL_Tankmeasure":
                    request.CommandType = "Get_LYQH_GJL_Tankmeasure";
                    parDic.Add(0, 2);
                    parDic.Add(1, slot);
                    break;
                #endregion

                #region 德汇
                case "Get_DEHUI_LLJ":  // 读取有线功图
                    request.CommandType = "Get_DEHUI_LLJ";
                    parDic.Add(0, 2);
                    break;
                    #endregion
            }

            request.CommandParameter = parDic.ToJson().IndentJson();
            return request.ToJson();
        }

        public static string MockLYQHRequest(LYQHRequest request, int modbusAddress = 1, int slot = -1)
        {
            var messageString = "";
            switch (request)
            {
                case LYQHRequest.Get_LYQH_DB_PowerMeter:
                    messageString = DebugHelper.MockRequest("Get_LYQH_DB_PowerMeter", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_DLT_CurrentDiagram:
                    messageString = DebugHelper.MockRequest("Get_LYQH_DLT_CurrentDiagram", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_WG_IndicatorDiagram:
                    messageString = DebugHelper.MockRequest("Get_LYQH_WG_IndicatorDiagram", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Post_LYQH_KZG_StartWell:
                    messageString = DebugHelper.MockRequest("Post_LYQH_KZG_StartWell", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_KZG_StopWell:
                    messageString = DebugHelper.MockRequest("POST_LYQH_KZG_StopWell", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_KZG_StartWellTime:
                    messageString = DebugHelper.MockRequest("Get_LYQH_KZG_StartWellTime", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_GLT_PowerDiagram:
                    messageString = DebugHelper.MockRequest("Get_LYQH_GLT_PowerDiagram", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_DYM_Movefluidlevel:
                    messageString = DebugHelper.MockRequest("Get_LYQH_DYM_Movefluidlevel", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.Get_LYQH_GJL_Tankmeasure:
                    messageString = DebugHelper.MockRequest("Get_LYQH_GJL_Tankmeasure", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_GTShiShiCaiJI:
                    messageString = DebugHelper.MockRequest("POST_LYQH_GTShiShiCaiJI", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_DB_ShiShiCaiJI:
                    messageString = DebugHelper.MockRequest("POST_LYQH_DB_ShiShiCaiJI", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_DYMShiShiCaiJI:
                    messageString = DebugHelper.MockRequest("POST_LYQH_DYMShiShiCaiJI", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_GJLShiShiCaiJI:
                    messageString = DebugHelper.MockRequest("POST_LYQH_GJLShiShiCaiJI", 1, modbusAddress, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_WG_IndicatorDiagramSetting:
                    messageString = DebugHelper.MockRequest("POST_LYQH_WG_IndicatorDiagramSetting", 1, 255, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_DYMSetting:
                    messageString = DebugHelper.MockRequest("POST_LYQH_DYMSetting", 1, 255, "Device", slot);
                    break;

                case LYQHRequest.POST_LYQH_GJLSetting:
                    messageString = DebugHelper.MockRequest("POST_LYQH_GJLSetting", 1, 255, "Device", slot);
                    break;
                case LYQHRequest.POST_LYQH_DBSetting:
                    messageString = DebugHelper.MockRequest("POST_LYQH_DBSetting", 1, 255, "Device", slot);
                    break;
            }
            return messageString;
        }
    }

    public enum LYQHRequest
    {
        /// <summary>
        /// 电参
        /// </summary>
        Get_LYQH_DB_PowerMeter,
        /// <summary>
        /// 电流
        /// </summary>
        Get_LYQH_DLT_CurrentDiagram,
        /// <summary>
        /// 功图
        /// </summary>
        Get_LYQH_WG_IndicatorDiagram,
        Post_LYQH_KZG_StartWell,
        POST_LYQH_KZG_StopWell,
        Get_LYQH_KZG_StartWellTime,
        /// <summary>
        /// 功率图
        /// </summary>
        Get_LYQH_GLT_PowerDiagram,
        /// <summary>
        /// 动液面
        /// </summary>
        Get_LYQH_DYM_Movefluidlevel,
        /// <summary>
        /// 液位罐
        /// </summary>

        Get_LYQH_GJL_Tankmeasure,
        /// <summary>
        /// 功图实时采集
        /// </summary>
        POST_LYQH_GTShiShiCaiJI,
        /// <summary>
        /// 动液面实时采集
        /// </summary>
        POST_LYQH_DYMShiShiCaiJI,
        /// <summary>
        /// 功图设置
        /// </summary>
        POST_LYQH_WG_IndicatorDiagramSetting,
        /// <summary>
        /// 动液面设置
        /// </summary>
        POST_LYQH_DYMSetting,
        /// <summary>
        /// 罐计量设置
        /// </summary>
        POST_LYQH_GJLSetting,
        /// <summary>
        /// 电参设置
        /// </summary>
        POST_LYQH_DBSetting,
        /// <summary>
        /// 电参实时采集
        /// </summary>
        POST_LYQH_DB_ShiShiCaiJI,
        POST_LYQH_GJLShiShiCaiJI
    }


    public class ControlRequest
    {
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceTypeId { get; set; }
        public string GroupName { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public int ModbusAddress { get; set; }
        public string CommandType { get; set; }
        public string CommandParameter { get; set; }
        public string SessionId { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool UseMockData { get; set; }
        public string MockData { get; set; }
        public int? LinkId { get; set; }
    }

    /// <summary>
    /// tcp工程目前只能传byte，临时使用ControlRequest_old类
    /// 后期tcp工程移除
    /// </summary>
    public class ControlRequest_old
    {
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceTypeId { get; set; }
        public string GroupName { get; set; }
        public string LinkMode { get; set; }  //TCP  or  RTU
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public byte ModbusAddress { get; set; }
        public string CommandType { get; set; }
        public string CommandParameter { get; set; }
        public string SessionId { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool UseMockData { get; set; }
        public string MockData { get; set; }
        public int? LinkId { get; set; }
    }

    public class ControlRequestDeHui : ControlRequest
    {
        public long StationId { set; get; }
        public string StationName { set; get; }
    }

    public class ControlResponse : WebApiResponse
    {
        public string SessionId { get; set; }
    }
}
