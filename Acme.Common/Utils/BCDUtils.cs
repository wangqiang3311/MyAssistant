using System;
using System.Collections.Generic;
using System.Text;

namespace Acme.Common.Utils
{
    public static class BCDUtils
    {
        public static ushort BCDToUshort(ushort input)
        {
            ushort outInt = 0;

            var bcd = new byte[2];
            bcd[1] = (byte)(input >> 8);
            bcd[0] = (byte)(input & 0xFF);
            for (int i = 0; i < 2; i++)
            {
                ushort mul = (ushort)Math.Pow(10, (i * 2));
                outInt += (ushort)(((bcd[i] & 0xF)) * mul);
                mul = (ushort)Math.Pow(10, (i * 2) + 1);
                outInt += (ushort)(((bcd[i] >> 4)) * mul);
            }
            return outInt;
        }

        public static ushort UshortToBCD(ushort input)
        {
            var bytesize = 2;
            byte[] bcd = new byte[2];
            for (ushort byteNo = 0; byteNo < bytesize; ++byteNo)
                bcd[byteNo] = 0;
            for (ushort digit = 0; digit < bytesize * 2; ++digit)
            {
                var hexpart = input % 10;
                bcd[digit / 2] |= (byte)(hexpart << ((digit % 2) * 4));
                input /= 10;
            }
            return (ushort)(bcd[1] << 8 | bcd[0]);
        }
    }
}
