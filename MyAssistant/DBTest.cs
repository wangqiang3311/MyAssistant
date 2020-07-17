using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;

namespace MyAssistant
{

     class DBTest
     {
          static void Test()
          {
               var appSettings = new AppSettings();
               var dbTypeSys = appSettings.Get<string>("SYS.DbType");
               var dbConnectionSys = appSettings.Get<string>("SYS.DbConnection");

               var dbFactory = new OrmLiteConnectionFactory(dbConnectionSys, MySqlDialect.Provider);
               using var dbFac = dbFactory.OpenDbConnection();

               var groupName = "SLS-30";
               //var waterStationList3 = dbFac.Select<ViewIotWaterStationDevice>(x => ((x.DeviceClass) == (int)(DeviceClass.WaterTrunkPressureMeter)) && x.GroupName == groupName && (x.Enable == true));

               //foreach (var item in waterStationList3)
               //{
               //     Console.WriteLine("linkId:" + item.LinkId);
               //}

               Console.Read();
          }
     }
}
