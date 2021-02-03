using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acme.Common
{
    public static class StringHelper
    {
        private static readonly char[] Base64CodeArray = new char[]
        {
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                    '0', '1', '2', '3', '4',  '5', '6', '7', '8', '9', '+', '/', '='
        };

        public static bool IsBase64(string base64Str)
        {
            byte[] bytes = null;

            if (string.IsNullOrEmpty(base64Str))
                return false;
            else
            {
                if (base64Str.Contains(","))
                    base64Str = base64Str.Split(',')[1];
                if (base64Str.Length % 4 != 0)
                    return false;
                if (base64Str.Any(c => !Base64CodeArray.Contains(c)))
                    return false;
            }
            try
            {
                bytes = Convert.FromBase64String(base64Str);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string BytesToHexString(byte[] bytes)
        {
            if (bytes == null) return "";
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2} ", b);
            }
            return sb.ToString().Trim();
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            string[] array = hexString.Split(' ',StringSplitOptions.RemoveEmptyEntries);

            List<byte> bytes = new List<byte>();

            string[] hex = { "A", "B", "C", "D", "E", "F" };

            foreach (var item in array)
            {
                if (item.StartsWith("0"))
                {
                    var newValue = item.TrimStart('0');

                    if (hex.Contains(newValue))
                    {
                        switch (newValue)
                        {
                            case "A":
                                newValue = "10";
                                break;
                            case "B":
                                newValue = "11";
                                break;
                            case "C":
                                newValue = "12";
                                break;
                            case "D":
                                newValue = "13";
                                break;
                            case "E":
                                newValue = "14";
                                break;
                            case "F":
                                newValue = "15";
                                break;
                        }
                        bytes.Add(byte.Parse(newValue));
                    }
                    else
                    {
                        if (newValue == "") newValue = "0";
                        var value1 = byte.Parse(newValue);

                        if (value1 < 10)
                        {
                            bytes.Add(value1);
                        }
                    }
                }
                else
                {
                    var chars = item.ToCharArray();

                    var first = chars[0].ToString();

                    if (hex.Contains(first))
                    {
                        switch (first)
                        {
                            case "A":
                                first = "10";
                                break;
                            case "B":
                                first = "11";
                                break;
                            case "C":
                                first = "12";
                                break;
                            case "D":
                                first = "13";
                                break;
                            case "E":
                                first = "14";
                                break;
                            case "F":
                                first = "15";
                                break;
                        }
                    }


                    var second = chars[1].ToString();

                    if (hex.Contains(second))
                    {
                        switch (second)
                        {
                            case "A":
                                second = "10";
                                break;
                            case "B":
                                second = "11";
                                break;
                            case "C":
                                second = "12";
                                break;
                            case "D":
                                second = "13";
                                break;
                            case "E":
                                second = "14";
                                break;
                            case "F":
                                second = "15";
                                break;
                        }
                    }

                    var high = byte.Parse(first);
                    var low = byte.Parse(second);

                    var value1 = high << 4 | low;
                    bytes.Add((byte)value1);
                }
            }

            return bytes.ToArray();
        }
    }
}
