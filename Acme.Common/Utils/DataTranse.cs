using System;
using System.Collections.Generic;
using System.Text;

namespace Acme.Common.Utils
{
    public class DataTranse
    {
        public static ushort ByteToUShort(byte[] source, int index)
        {
            var s1 = source[index];
            var s2 = source[index + 1];

            var s3 = s1 << 8 | s2;
            return (ushort)s3;
        }

        public static byte[] ShortToByte(short s)
        {
            byte[] binfo = new byte[2];
            binfo[0] = (byte)(s >> 8);
            binfo[1] = (byte)s;
            return binfo;
        }
        /// <summary>
        /// CDAB转换
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] IntToByteForCDAB(int s)
        {
            byte[] binfo = new byte[4];
            binfo[0] = (byte)(s >> 8);
            binfo[1] = (byte)s;
            binfo[2] = (byte)(s >> 24);
            binfo[3] = (byte)(s >> 16);
            return binfo;
        }

        public static byte[] IntToByte(int s)
        {
            byte[] binfo = new byte[4];
           
            binfo[0] = (byte)(s >> 24);
            binfo[1] = (byte)(s >> 16);
            binfo[2] = (byte)(s >> 8);
            binfo[3] = (byte)s;

            return binfo;
        }
    }
}
