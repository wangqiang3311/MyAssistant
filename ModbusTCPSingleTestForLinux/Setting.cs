using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusTCPTest
{
    public class Setting
    {
        public List<Vendor> vendor { get; set; }
    }

    public class Vendor
    {
        public string name { set; get; }
        public string showName { set; get; }

        public string[] commands { set; get; }
    }
}
