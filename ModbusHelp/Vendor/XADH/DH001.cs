//using Modbus.Common;
//using Modbus.Common.Structures;
//using Modbus.Tcp.Client;
//using ServiceStack;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace WellMonitor.ModbusReaderWriter.Vendor
//{
//    public class AddDAT010 : DAT010
//    {
//    }

//    public class ControlRequest_DH001_READ_ALL : ControlRequest
//    {

//    }

//    public class ControlRequest_DH001_SET_DEVICE_CONFIG : ControlRequest
//    {
//        public float DCRJ { get; set; }     //单仓容积
//        public float DMJ { get; set; }      //底面积
//        public float YZG { get; set; }      //液柱高
//        public float XGXS { get; set; }     //相关系数
//        public int ZDJS { get; set; }       //最大井数
//        public int SZXS { get; set; }       //设置小时
//        public int SZFZ { get; set; }       //设置分钟
//        public int ZDQJZQ { get; set; }     //自动切井周期
//        public float QDWD { get; set; }     //启动温度
//        public float TZWD { get; set; }     //停止温度
//        public float SDND1 { get; set; }      //设定浓度1
//        public float SDND2 { get; set; }      //设定浓度2
//    }


//    public class ControlRequest_DH001_SWITCH_WELL : ControlRequest
//    {
//        public int SDJ1 { get; set; }  //手动井一
//        public int SDJ2 { get; set; }  //手动井二
//        public int SDJ3 { get; set; }  //手动井三
//        public int SDJ4 { get; set; }  //手动井四
//        public int SDJ5 { get; set; }  //手动井五
//        public int SDJ6 { get; set; }  //手动井六
//        public int SDJ7 { get; set; }  //手动井七
//        public int SDJ8 { get; set; }  //手动井八
//        public int QJZD { get; set; }  //切井自动
//        public int WXAN { get; set; }  //维修按钮
//        public int DSPD { get; set; }  //定时巡井
//        public int XJCY { get; set; }  //循井测液
//    }

//    // DAT010 计量设备状态表
//    public class DAT010
//    {
//        public long Id { get; set; }         //Id
//        public string SessionId { get; set; }  //SesssionID
//        public int JLID { get; set; }          //设备ID
//        public DateTime DQSJ { get; set; }     //读取时间
//        public int CZY { get; set; }    //操作员
//        public int KDWJY { get; set; }  //开到位_进液
//        public int GDWJY { get; set; }  //关到位_进液
//        public int KDWCY { get; set; }  //开到位_出液
//        public int GDWCY { get; set; }  //关到位_出液
//        public int KDWJ1 { get; set; }  //开到位_井1
//        public int GDWJ1 { get; set; }  //关到位_井1
//        public int KDWJ2 { get; set; }  //开到位_井2
//        public int GDWJ2 { get; set; }  //关到位_井2
//        public int KDWJ3 { get; set; }  //开到位_井3
//        public int GDWJ3 { get; set; }  //关到位_井3
//        public int KDWJ4 { get; set; }  //开到位_井4
//        public int GDWJ4 { get; set; }  //关到位_井4
//        public int KDWJ5 { get; set; }  //开到位_井5
//        public int GDWJ5 { get; set; }  //关到位_井5
//        public int KDWJ6 { get; set; }  //开到位_井6
//        public int GDWJ6 { get; set; }   //关到位_井6
//        public int KDWJ7 { get; set; }   //开到位_井7
//        public int GDWJ7 { get; set; }   //关到位_井7
//        public int YLKG { get; set; }    //压力开关
//        public int BDJ1 { get; set; }    //本地-井1
//        public int BDJ2 { get; set; }    //本地-井2
//        public int BDJ3 { get; set; }    //本地-井3
//        public int BDJ4 { get; set; }    //本地-井4
//        public int BDJ5 { get; set; }    //本地-井5
//        public int BDJ6 { get; set; }    //本地-井6
//        public int BDJ7 { get; set; }    //本地-井7
//        public int BDSZJH { get; set; }  //本地-手自交换
//        public int ABTQH { get; set; }   //A/B塔切换
//        public int ZDCQ { get; set; }    //制氮充气
//        public int JRZ { get; set; }     //加热左
//        public int JRY { get; set; }     //加热右
//        public int FS { get; set; }      //风扇
//        public float LYWD { get; set; }  //来液温度
//        public float CNYL { get; set; }  //仓内压力
//        public float ZCYW { get; set; }  //左仓液位
//        public float YCYW { get; set; }  //右仓液位
//        public float HJWD { get; set; }  //环境温度
//        public float SSHS { get; set; }  //瞬时含水
//        public float SBND { get; set; }  //上部浓度
//        public float XBND { get; set; }  //下部浓度
//        public float LJCY { get; set; }  //累计产液
//        public float SSCY { get; set; }  //实时产液
//        public float SSPJHS { get; set; }  //实时平均含水
//        public float ZRHS { get; set; }  //昨日含水
//        public float ZFCY { get; set; }  //昨日产液
//        public int DQJLQLX { get; set; }  //读取计量器类型
//        public int ZRJH { get; set; }  //昨日井号
//        public int SSJH { get; set; }  //实时井号
//        public int XJSQXS { get; set; }  //巡井手切小时
//        public int XJSQFZ { get; set; }  //巡井手切分钟
//        public int CQLJFZS { get; set; }  //充气/起泵累计分钟数
//        public int JRCS { get; set; }  //加热器次数
//        public int FSCS { get; set; }  //风扇次数
//        public int JSYZTM { get; set; }  //状态码：井上液
//        public int FMZTM { get; set; }  //状态码：阀门/(浮球/泵)
//        public int DQPLCBH { get; set; }  //读取PLC编号
//        public int QJZD { get; set; }  //切井自动
//        public int WXAN { get; set; }  //维修按钮
//        public int DSPD { get; set; }  //定时巡井
//        public int XJCY { get; set; }  //循井测液
//        public int SDJ1 { get; set; }  //手动井一
//        public int SDJ2 { get; set; }  //手动井二
//        public int SDJ3 { get; set; }  //手动井三
//        public int SDJ4 { get; set; }  //手动井四
//        public int SDJ5 { get; set; }  //手动井五
//        public int SDJ6 { get; set; }  //手动井六
//        public int SDJ7 { get; set; }  //手动井七
//        public int SDJ8 { get; set; }  //手动井八

//        public float DCRJ { get; set; }     //单仓容积
//        public float DMJ { get; set; }      //底面积
//        public float YZG { get; set; }      //液柱高
//        public float XGXS { get; set; }     //相关系数
//        public int ZDJS { get; set; }       //最大井数
//        public int SZXS { get; set; }       //设置小时
//        public int SZFZ { get; set; }       //设置分钟
//        public int ZDQJZQ { get; set; }     //自动切井周期
//        public float QDWD { get; set; }     //启动温度
//        public float TZWD { get; set; }     //停止温度
//        public int SDND1 { get; set; }      //设定浓度1
//        public int SDND2 { get; set; }      //设定浓度2
//    }

//    public static class DH001
//    {
//        public static async Task DH001_READ_ALL(ModbusClient client, JsonServiceClient jsonServiceClient, string messageString)
//        {
//            var config = messageString.FromJson<ControlRequest_DH001_READ_ALL>();
//            var modbusAddress = config.ModbusAddress;
//            var dat101 = new DAT010();

//            var result1 = await client.ReadDiscreteInputs(modbusAddress, 1025 - 1, 1055 - 1025 + 1);
//            Console.WriteLine("ReadDiscreteInputs: " + result1.Count);

//            //1   X0 开到位_进液  101025  离散 只读
//            dat101.KDWJY = result1[0].Value ? 1 : 0;
//            Console.WriteLine("开到位_进液  101025: " + result1[0].Value);
//            //2   X1 关到位_进液  101026  离散 只读
//            dat101.GDWJY = result1[1].Value ? 1 : 0;
//            Console.WriteLine("关到位_进液  101026: " + result1[1].Value);
//            //3   X2 开到位_出液  101027  离散 只读
//            dat101.KDWCY = result1[2].Value ? 1 : 0;
//            Console.WriteLine("开到位_出液  101027: " + result1[2].Value);
//            //4   X3 关到位_出液  101028  离散 只读
//            dat101.GDWCY = result1[3].Value ? 1 : 0;
//            Console.WriteLine("关到位_出液  101028: " + result1[3].Value);
//            //5   X4 开到位_井1  101029  离散 只读
//            dat101.KDWJ1 = result1[4].Value ? 1 : 0;
//            Console.WriteLine("开到位_井1   101029: " + result1[4].Value);
//            //6   X5 关到位_井1  101030  离散 只读
//            dat101.GDWJ1 = result1[5].Value ? 1 : 0;
//            Console.WriteLine("关到位_井1  101030: " + result1[5].Value);
//            //7   X6 开到位_井2  101031  离散 只读
//            dat101.KDWJ2 = result1[6].Value ? 1 : 0;
//            Console.WriteLine("开到位_井2  101031: " + result1[6].Value);
//            //8   X7 关到位_井2  101032  离散 只读
//            dat101.GDWJ2 = result1[7].Value ? 1 : 0;
//            Console.WriteLine("关到位_井2  101032: " + result1[7].Value);
//            //9   X10 开到位_井3  101033  离散 只读
//            dat101.KDWJ3 = result1[8].Value ? 1 : 0;
//            Console.WriteLine("开到位_井3  101033: " + result1[8].Value);
//            //10  X11 关到位_井3  101034  离散 只读
//            dat101.GDWJ3 = result1[9].Value ? 1 : 0;
//            Console.WriteLine("关到位_井3  101034: " + result1[9].Value);
//            //11  X12 开到位_井4  101035  离散 只读
//            dat101.KDWJ4 = result1[10].Value ? 1 : 0;
//            Console.WriteLine("开到位_井4  101035: " + result1[10].Value);
//            //12  X13 关到位_井4  101036  离散 只读
//            dat101.GDWJ4 = result1[11].Value ? 1 : 0;
//            Console.WriteLine("关到位_井4  101036: " + result1[11].Value);
//            //13  X14 开到位_井5  101037  离散 只读
//            dat101.KDWJ5 = result1[12].Value ? 1 : 0;
//            Console.WriteLine("开到位_井5  101037: " + result1[12].Value);
//            //14  X15 关到位_井5  101038  离散 只读
//            dat101.GDWJ5 = result1[13].Value ? 1 : 0;
//            Console.WriteLine("关到位_井5  101038: " + result1[13].Value);
//            //15  X16 开到位_井6  101039  离散 只读
//            dat101.KDWJ6 = result1[14].Value ? 1 : 0;
//            Console.WriteLine("开到位_井6  101039: " + result1[14].Value);
//            //16  X17 关到位_井6  101040  离散 只读
//            dat101.GDWJ6 = result1[15].Value ? 1 : 0;
//            Console.WriteLine("关到位_井6  101040: " + result1[15].Value);
//            //17  X20 开到位_井7  101041  离散 只读
//            dat101.KDWJ7 = result1[16].Value ? 1 : 0;
//            Console.WriteLine("开到位_井7  101041: " + result1[16].Value);
//            //18  X21 关到位_井7  101042  离散 只读
//            dat101.GDWJ7 = result1[17].Value ? 1 : 0;
//            Console.WriteLine("关到位_井7  101042: " + result1[17].Value);
//            //19  X22 开到位_井8  101043  离散 只读
//            //dat101.KDWJ = result[18].Value ? 1 : 0;
//            //Console.WriteLine("开到位_井8  101043:" + result1[18].Value);

//            //21  X23 压力开关  101044  离散 只读
//            dat101.YLKG = result1[18].Value ? 1 : 0;
//            Console.WriteLine("压力开关  101044: " + result1[19].Value);

//            //21  X24 本地-井1  101045  离散 只读
//            dat101.BDJ1 = result1[19].Value ? 1 : 0;
//            Console.WriteLine("本地-井1  101045: " + result1[20].Value);

//            //21  X25 本地-井2  101046  离散 只读
//            dat101.BDJ2 = result1[20].Value ? 1 : 0;
//            Console.WriteLine("本地-井2  101046: " + result1[21].Value);

//            //21  X26 本地-井3  101047  离散 只读
//            dat101.BDJ3 = result1[21].Value ? 1 : 0;
//            Console.WriteLine("本地-井3  101047: " + result1[22].Value);

//            //21  X27 本地-井4  101048  离散 只读
//            dat101.BDJ4 = result1[22].Value ? 1 : 0;
//            Console.WriteLine("本地-井4  101048: " + result1[23].Value);

//            //21  X30 本地-井5  101049  离散 只读
//            dat101.BDJ5 = result1[23].Value ? 1 : 0;
//            Console.WriteLine("本地-井5  101049: " + result1[24].Value);

//            //21  X31 本地-井6  101050  离散 只读
//            dat101.BDJ6 = result1[24].Value ? 1 : 0;
//            Console.WriteLine("本地-井6  101050: " + result1[25].Value);

//            //21  X32 本地-井7  101051  离散 只读
//            dat101.BDJ7 = result1[25].Value ? 1 : 0;
//            Console.WriteLine("本地-井3  101051: " + result1[26].Value);

//            //21  X33 本地-手自交换  101052  离散 只读
//            dat101.BDSZJH = result1[26].Value ? 1 : 0;
//            Console.WriteLine("本地-手自交换 101052: " + result1[27].Value);

//            Console.WriteLine("");

//            var result3 = await client.ReadCoils(modbusAddress, 2051 - 1, 2058 - 2051 + 1);
//            Console.WriteLine("ReadCoils: " + result3.Count);

//            dat101.SDJ1 = result3[0].Value ? 1 : 0;
//            Console.WriteLine("手动井1  002051: " + result3[0].Value);
//            //37  M3 手动井2    002052  离散 读写
//            dat101.SDJ2 = result3[1].Value ? 1 : 0;
//            Console.WriteLine("手动井2  002052: " + result3[1].Value);
//            //38  M4 手动井3    002053  离散 读写
//            dat101.SDJ3 = result3[2].Value ? 1 : 0;
//            Console.WriteLine("手动井3  002053: " + result3[2].Value);
//            //39  M5 手动井4    002054  离散 读写
//            dat101.SDJ4 = result3[3].Value ? 1 : 0;
//            Console.WriteLine("手动井4  002054: " + result3[3].Value);
//            //40  M6 手动井5    002055  离散 读写
//            dat101.SDJ5 = result3[4].Value ? 1 : 0;
//            Console.WriteLine("手动井5  002055: " + result3[4].Value);
//            //41  M7 手动井6    002056  离散 读写
//            dat101.SDJ6 = result3[5].Value ? 1 : 0;
//            Console.WriteLine("手动井6  002056: " + result3[5].Value);
//            //42  M8 手动井7    002057  离散 读写
//            dat101.SDJ7 = result3[6].Value ? 1 : 0;
//            Console.WriteLine("手动井7  002057: " + result3[6].Value);
//            //43  M9 手动井8    002058  离散 读写
//            dat101.SDJ8 = result3[7].Value ? 1 : 0;
//            Console.WriteLine("手动井8  002058: " + result3[7].Value);

//            Console.WriteLine("");

//            var result4 = await client.ReadCoils(modbusAddress, 2569 - 1, 2572 - 2569 + 1);
//            Console.WriteLine("ReadCoils: " + result4.Count);

//            //54  M520 切井自动    002569  离散 读写
//            dat101.QJZD = result4[0].Value ? 1 : 0;
//            Console.WriteLine("切井自动  002569: " + result4[0].Value);
//            //55  M521 维修按钮    002570  离散 读写
//            dat101.WXAN = result4[1].Value ? 1 : 0;
//            Console.WriteLine("维修按钮  002570: " + result4[1].Value);
//            //56  M522 定时判断    002571  离散 读写
//            dat101.DSPD = result4[2].Value ? 1 : 0;
//            Console.WriteLine("定时判断  002571: " + result4[2].Value);
//            //57  M523 循井按钮    002572  离散 读写
//            dat101.XJCY = result4[3].Value ? 1 : 0;
//            Console.WriteLine("循井按钮  002572: " + result4[3].Value);

//            Console.WriteLine("");

//            var result6 = await client.ReadHoldingRegisters(modbusAddress, 4097 - 1, 4112 - 4097 + 1);
//            Console.WriteLine("ReadHoldingRegisters:" + result6.Count);

//            //61  D0 来液温度      404097  浮点型 只读
//            //62  D1              404098  浮点型 只读
//            dat101.LYWD = result6.GetSingle(0);
//            Console.WriteLine("来液温度  404097:" + result6.GetSingle(0));

//            //63  D2 仓内压力      404099  浮点型 只读
//            //64  D3              404100  浮点型 只读
//            dat101.CNYL = result6.GetSingle(2);
//            Console.WriteLine("仓内压力  404099:" + result6.GetSingle(2));

//            //65  D4 左仓液位      404101  浮点型 只读
//            //66  D5              404102  浮点型 只读
//            dat101.ZCYW = result6.GetSingle(4);
//            Console.WriteLine("左仓液位  404101:" + result6.GetSingle(4));

//            ////67  D6 右仓液位     404103  浮点型 只读
//            ////68  D7              404104  浮点型 只读
//            dat101.YCYW = result6.GetSingle(6);
//            Console.WriteLine("右仓液位  404103:" + result6.GetSingle(6));

//            //69  D8 环境温度      404105  浮点型 只读
//            //70  D9              404106  浮点型 只读
//            dat101.HJWD = result6.GetSingle(8);
//            Console.WriteLine("环境温度  404105:" + result6.GetSingle(8));

//            //71  D10 瞬时含水     404107  浮点型 只读
//            //72  D11             404108  浮点型 只读
//            dat101.SBND = result6.GetSingle(10);
//            Console.WriteLine("瞬时含水  404107:" + result6.GetSingle(10));

//            //71  D10 上部浓度     404109  浮点型 只读
//            //72  D11             404110  浮点型 只读
//            dat101.SBND = result6.GetSingle(12);
//            Console.WriteLine("上部浓度  404109:" + result6.GetSingle(12));

//            //73  D12 下部浓度     404111  浮点型 只读
//            //74  D13             404112  浮点型 只读
//            dat101.XBND = result6.GetSingle(14);
//            Console.WriteLine("下部浓度  404111:" + result6.GetSingle(14));

//            Console.WriteLine("");

//            var result7 = await client.ReadHoldingRegisters(modbusAddress, 4505 - 1, 4550 - 4505 + 1);
//            Console.WriteLine("ReadHoldingRegisters:" + result7.Count);

//            //93  D408 累计产液    404505  浮点型 只读
//            //94  D409            404506  浮点型 只读
//            dat101.LJCY = result7.GetSingle(0);
//            Console.WriteLine("累计产液  404505:" + result7.GetSingle(0));

//            //95  D410 单仓容积    404507  浮点型 读写
//            //96  D411            404508  浮点型 读写
//            dat101.DCRJ = result7.GetSingle(2);
//            Console.WriteLine("单仓容积  404507:" + result7.GetSingle(2));

//            //97  D412 实时产液    404509  浮点型 只读
//            //98  D413            404510  浮点型 只读
//            dat101.SSCY = result7.GetSingle(4);
//            Console.WriteLine("实时产液  404509:" + result7.GetSingle(4));

//            //101 D416 实时平均含水 404511  浮点型 只读
//            //102 D417             404512  浮点型 只读
//            dat101.SSPJHS = result7.GetSingle(6);
//            Console.WriteLine("实时平均含水  404511:" + result7.GetSingle(6));

//            //103 D418 昨日含水    404513  浮点型 只读
//            //104 D419            404514  浮点型 只读
//            dat101.ZRHS = result7.GetSingle(8);
//            Console.WriteLine("昨日含水  404513:" + result7.GetSingle(8));

//            //保留一个浮点型  2个寄存器

//            //105 D420 昨日产液    404517  浮点型 只读
//            //106 D421            404518  浮点型 只读
//            dat101.ZFCY = result7.GetSingle(12);
//            Console.WriteLine("昨日产液  404517:" + result7.GetSingle(12));

//            //保留一个浮点型  2个寄存器
//            //保留一个整型    1个寄存器

//            //105 D420 读取计量器类型   404522  整型 只读
//            Console.WriteLine("读取计量器类型  404522:" + result7[17].Value);
//            dat101.DQJLQLX = result7[17].Value;

//            //113 D428 底面积     404523  浮点型 读写
//            //114 D429            404524  浮点型 读写
//            dat101.DMJ = result7.GetSingle(18);
//            Console.WriteLine("底面积  404523:" + result7.GetSingle(18));

//            //113 D428 液柱高      404525  浮点型 读写
//            //114 D429            404526  浮点型 读写
//            dat101.YZG = result7.GetSingle(20);
//            Console.WriteLine("液柱高  404525:" + result7.GetSingle(20));

//            //115 D430 相关系数    404527  浮点型 读写
//            //116 D431            404528  浮点型 读写
//            dat101.XGXS = result7.GetSingle(22);
//            Console.WriteLine("相关系数  404527:" + result7.GetSingle(22));

//            //117 D432 昨日井号    404529  整型 只读
//            dat101.ZRJH = result7[24].Value;
//            Console.WriteLine("昨日井号  404529:" + result7[24].Value);

//            //118 D433 实时井号    404530  整型 只读
//            dat101.SSJH = result7[25].Value;
//            Console.WriteLine("实时井号  404530:" + result7[25].Value);

//            //119 D434 最大井数    404531  整型 读写
//            dat101.ZDJS = result7[26].Value;
//            Console.WriteLine("最大井数  404531:" + result7[26].Value);

//            //120 D435 设置小时    404532  整型 读写
//            dat101.SZXS = result7[27].Value;
//            Console.WriteLine("设置小时  404532:" + result7[27].Value);

//            //121 D436 设置分钟    404533  整型 读写
//            dat101.SZFZ = result7[28].Value;
//            Console.WriteLine("设置分钟  404533:" + result7[28].Value);

//            //122 D437 巡井手切小时  404534  整型 只读
//            dat101.XJSQXS = result7[29].Value;
//            Console.WriteLine("巡井手切小时  404534:" + result7[29].Value);

//            //123 D438 巡井手切分钟  404535  整型 只读
//            dat101.XJSQFZ = result7[30].Value;
//            Console.WriteLine("巡井手切分钟  404535:" + result7[30].Value);

//            //124 D439 助排泵次数    404536  整型 只读
//            dat101.CQLJFZS = result7[31].Value;
//            Console.WriteLine("助排泵次数  404536:" + result7[31].Value);

//            //125 D440 加热器次数    404537  整型 只读
//            dat101.JRCS = result7[32].Value;
//            Console.WriteLine("加热器次数  404537:" + result7[32].Value);

//            //126 D441 风扇次数      404538  整型 只读
//            dat101.FSCS = result7[33].Value;
//            Console.WriteLine("风扇次数  404538:" + result7[33].Value);

//            //127 D442 状态码：井上液 404539  整型 只读
//            dat101.JSYZTM = result7[34].Value;
//            Console.WriteLine("状态码：井上液  404539:" + result7[34].Value);

//            //128 D443 状态码: 阀泵秋 404540  整型 只读
//            dat101.FMZTM = result7[35].Value;
//            Console.WriteLine("状态码: 阀泵秋  404540:" + result7[35].Value);

//            //105 D444 自动切井周期   404541  整型 只读
//            dat101.ZDQJZQ = result7[36].Value;
//            Console.WriteLine("自动切井周期  404541:" + result7[36].Value);

//            //105 D445 读取plc编号   404542  整型 只读
//            dat101.DQPLCBH = result7[37].Value;
//            Console.WriteLine("读取plc编号  404542:" + result7[37].Value);

//            //131 D446 启动温度    404543  浮点型 读写
//            //132 D447            404544  浮点型 读写
//            dat101.QDWD = result7.GetSingle(38);
//            Console.WriteLine("启动温度  404543:" + result7.GetSingle(38));

//            //133 D448 停止温度    404545  浮点型 读写
//            //134 D449            404546  浮点型 读写
//            dat101.TZWD = result7.GetSingle(40);
//            Console.WriteLine("停止温度  404545:" + result7.GetSingle(40));

//            //135 D450 设定浓度1   404547  浮点型 读写
//            //136 D451            404548  浮点型 读写
//            dat101.SBND = result7.GetSingle(42);
//            Console.WriteLine("设定浓度1  404547:" + result7.GetSingle(42));

//            //137 D452 设定浓度2   404549  浮点型 读写
//            //138 D453            404550  浮点型 读写
//            dat101.XBND = result7.GetSingle(44);
//            Console.WriteLine("设定浓度2  404549:" + result7.GetSingle(44));

//            Console.WriteLine("");

//            dat101.Id = config.DeviceId.ToInt();
//            dat101.JLID = config.DeviceId.ToInt();
//            dat101.DQSJ = DateTime.Now;
//            dat101.SessionId = config.SessionId;


//            //Console.WriteLine(loginResponse.ToJson());
//            var response = jsonServiceClient.Post<WebApiResponse>(AutoMappingUtils.ConvertTo<AddDAT010>(dat101));
//            Console.WriteLine(response.ToJson());
//        }

//        public static async Task DH001_SWITCH_WELL(ModbusClient client, JsonServiceClient jsonServiceClient, string messageString)
//        {
//            var config = messageString.FromJson<ControlRequest_DH001_SWITCH_WELL>();
//            var modbusAddress = config.ModbusAddress;

//            //public int SDJ1 { get; set; }  //手动井一
//            var coil = new Coil
//            {
//                Address = 2051 - 1,
//                Value = config.SDJ1 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井一: " + coil.Value);

//            //public int SDJ2 { get; set; }  //手动井二
//            coil = new Coil
//            {
//                Address = 2052 - 1,
//                Value = config.SDJ2 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井二 " + coil.Value);

//            //public int SDJ3 { get; set; }  //手动井三
//            coil = new Coil
//            {
//                Address = 2053 - 1,
//                Value = config.SDJ3 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井三: " + coil.Value);

//            //public int SDJ4 { get; set; }  //手动井四
//            coil = new Coil
//            {
//                Address = 2054 - 1,
//                Value = config.SDJ4 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井四: " + coil.Value);

//            //public int SDJ5 { get; set; }  //手动井五
//            coil = new Coil
//            {
//                Address = 2055 - 1,
//                Value = config.SDJ5 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井五: " + coil.Value);

//            //public int SDJ6 { get; set; }  //手动井六
//            coil = new Coil
//            {
//                Address = 2056 - 1,
//                Value = config.SDJ6 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井六: " + coil.Value);

//            //public int SDJ7 { get; set; }  //手动井七
//            coil = new Coil
//            {
//                Address = 2057 - 1,
//                Value = config.SDJ7 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井七: " + coil.Value);

//            //public int SDJ8 { get; set; }  //手动井八
//            coil = new Coil
//            {
//                Address = 2058 - 1,
//                Value = config.SDJ8 == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("手动井八: " + coil.Value);

//            //public int QJZD { get; set; }  //切井自动
//            coil = new Coil
//            {
//                Address = 2569 - 1,
//                Value = config.QJZD == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("切井自动: " + coil.Value);

//            //public int WXAN { get; set; }  //维修按钮
//            coil = new Coil
//            {
//                Address = 2570 - 1,
//                Value = config.WXAN == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("维修按钮: " + coil.Value);

//            //public int DSPD { get; set; }  //定时巡井
//            coil = new Coil
//            {
//                Address = 2571 - 1,
//                Value = config.DSPD == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("定时巡井: " + coil.Value);

//            //public int XJCY { get; set; }  //循井测液
//            coil = new Coil
//            {
//                Address = 2572 - 1,
//                Value = config.XJCY == 0 ? false : true
//            };
//            await client.WriteSingleCoil(modbusAddress, coil);
//            Console.WriteLine("循井测液: " + coil.Value);
//        }

//        public static async Task DH001_SET_DEVICE_CONFIG(ModbusClient client, JsonServiceClient jsonServiceClient, string messageString)
//        {
//            var deviceConfig = messageString.FromJson<ControlRequest_DH001_SET_DEVICE_CONFIG>();
//            var modbusAddress = deviceConfig.ModbusAddress;
//            var registers = new List<Modbus.Common.Structures.Register>();

//            //public float DCRJ { get; set; }     //单仓容积
//            var r = Modbus.Common.Structures.Register.Create(deviceConfig.DCRJ, 4507 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("单仓容积: " + deviceConfig.DCRJ);
//            await client.WriteRegisters(modbusAddress, registers);

//            registers.Clear();
//            //public float DMJ { get; set; }      //底面积
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.DMJ, 4523 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("底面积: " + deviceConfig.DMJ);
//            //public float YZG { get; set; }      //液柱高
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.YZG, 4525 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("液柱高: " + deviceConfig.YZG);
//            //public float XGXS { get; set; }     //相关系数
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.XGXS, 4527 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("相关系数: " + deviceConfig.XGXS);
//            await client.WriteRegisters(modbusAddress, registers);

//            registers.Clear();
//            //public int ZDJS { get; set; }       //最大井数
//            var r1 = Modbus.Common.Structures.Register.Create((ushort)(deviceConfig.ZDJS), 4531 - 1);
//            registers.Add(r1);
//            Console.WriteLine("最大井数: " + deviceConfig.ZDJS);

//            //public int SZXS { get; set; }       //设置小时
//            r1 = Modbus.Common.Structures.Register.Create((ushort)(deviceConfig.SZXS), 4532 - 1);
//            registers.Add(r1);
//            Console.WriteLine("设置小时: " + deviceConfig.SZXS);

//            //public int SZFZ { get; set; }       //设置分钟
//            r1 = Modbus.Common.Structures.Register.Create((ushort)(deviceConfig.SZFZ), 4533 - 1);
//            registers.Add(r1);
//            Console.WriteLine("设置分钟: " + deviceConfig.SZFZ);

//            await client.WriteRegisters(modbusAddress, registers);

//            registers.Clear();
//            //public int ZDQJZQ { get; set; }     //自动切井周期
//            r1 = Modbus.Common.Structures.Register.Create((ushort)(deviceConfig.ZDQJZQ), 4541 - 1);
//            registers.Add(r1);
//            Console.WriteLine("自动切井周期: " + deviceConfig.ZDQJZQ);

//            await client.WriteRegisters(modbusAddress, registers);

//            registers.Clear();
//            //public float QDWD { get; set; }     //启动温度
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.QDWD, 4543 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("启动温度: " + deviceConfig.QDWD);

//            //public float TZWD { get; set; }     //停止温度
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.TZWD, 4545 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("停止温度: " + deviceConfig.TZWD);

//            //public int SDND1 { get; set; }      //设定浓度1
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.SDND1, 4547 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("设定浓度1: " + deviceConfig.SDND1);

//            //public int SDND2 { get; set; }      //设定浓度2
//            r = Modbus.Common.Structures.Register.Create(deviceConfig.SDND2, 4549 - 1);
//            registers.AddRange(r);
//            Console.WriteLine("设定浓度2: " + deviceConfig.SDND2);

//            await client.WriteRegisters(modbusAddress, registers);
//        }
//    }
//}
