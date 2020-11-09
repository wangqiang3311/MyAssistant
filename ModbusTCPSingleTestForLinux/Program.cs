using Microsoft.Extensions.Configuration;
using ModbusTCPTest;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YCIOT.ModbusPoll.RtuOverTcp;

namespace ModbusTCPSingleTestForLinux
{
    class Program
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static Setting setting;


        public static void Main(string[] args)
        {
            //添加 json 文件路径
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("setting.json");
            //创建配置根对象
            var configuration = builder.Build();

            setting = configuration.Get<Setting>();

            var vendors = setting.vendor.Select(v => new { v.name, v.showName });

            Dictionary<string, dynamic> vendorDics = new Dictionary<string, dynamic>();
            Dictionary<string, string> commandDics = new Dictionary<string, string>();

            while (true)
            {
                try
                {
                    vendorDics.Clear();
                    commandDics.Clear();

                    Console.WriteLine("vendor list:");

                    int i = 0;

                    foreach (var item in vendors)
                    {
                        i++;
                        vendorDics.Add(i.ToString(), item);
                        Logger.Info($"{i}:{item.showName}");
                    }

                    Console.WriteLine("please choose vendor:");

                    var vendorIndex = Console.ReadLine();

                    if (vendorDics.Keys.Contains(vendorIndex))
                    {
                        var vendorItem = vendorDics[vendorIndex];

                        var vendor = setting.vendor.SingleOrDefault(v => v.name == vendorItem.name);

                        if (vendor != null)
                        {
                            //Load vendor command

                            int j = 0;

                            foreach (var item in vendor.commands)
                            {
                                j++;

                                Console.WriteLine($"{j}:{item}");
                                commandDics.Add(j.ToString(), item);
                            }

                            Console.WriteLine("please choose command:");

                            var commandIndex = Console.ReadLine();

                            if (commandDics.Keys.Contains(commandIndex))
                            {
                                var commandType = commandDics[commandIndex];

                                Console.WriteLine("please input modbusAddress:");

                                var modbusAddress = Console.ReadLine();

                                Console.WriteLine("please input linkId:");

                                var linkId = Console.ReadLine();

                                Console.WriteLine("please input slotId:");

                                var slotId = Console.ReadLine();

                                Core.DoWork(commandType, int.Parse(linkId), int.Parse(slotId), int.Parse(modbusAddress));

                                Logger.Info("single test on working...");

                                Console.WriteLine("Is Contitue? Press Any key");

                                Console.ReadKey();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    break;
                }
            }
            Console.Read();
        }
    }
}
