using System;
using System.Linq;
using System.Text;

namespace Acme.Common
{
    public class Base64Image
    {
        public string ContentType { get; set; }

        public byte[] FileContents { get; set; }

        public static Base64Image Parse(string base64Content)
        {
            if (string.IsNullOrEmpty(base64Content))
                throw new ArgumentNullException(nameof(base64Content));
            var length = base64Content.IndexOf(";", StringComparison.OrdinalIgnoreCase);
            var str = base64Content.Substring(0, length).Split(':').Last();
            var startIndex = base64Content.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;
            var numArray = Convert.FromBase64String(base64Content.Substring(startIndex));
            return new Base64Image
            {
                ContentType = str,
                FileContents = numArray
            };
        }

        public override string ToString()
        {
            return "data:" + ContentType + ";base64," + Convert.ToBase64String(FileContents);
        }
    }

    public class Base64Helper
    {
        private static readonly char[] Base64CodeArray = new char[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4',  '5', '6', '7', '8', '9', '+', '/', '='
        };

        /// <summary>
        /// 是否base64字符串
        /// </summary>
        /// <param name="base64Str">要判断的字符串</param>
        /// <returns></returns>
        public static bool IsBase64(string base64Str)
        {
            byte[] bytes = null;
            return IsBase64(base64Str, out bytes);
        }

        /// <summary>
        /// 是否base64字符串
        /// </summary>
        /// <param name="base64Str">要判断的字符串</param>
        /// <param name="bytes">字符串转换成的字节数组</param>
        /// <returns></returns>
        public static bool IsBase64(string base64Str, out byte[] bytes)
        {
            //string strRegex = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$";
            bytes = null;
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

        public static string Base64Encode(Encoding encodeType, string source)
        {
            var bytes = encodeType.GetBytes(source);
            string str;
            try
            {
                str = Convert.ToBase64String(bytes);
            }
            catch
            {
                str = source;
            }

            return str;
        }

        public static string Base64Decode(string result)
        {
            return Base64Decode(Encoding.UTF8, result);
        }

        public static string Base64Decode(Encoding encodeType, string result)
        {
            var bytes = Convert.FromBase64String(result);
            string str;
            try
            {
                str = encodeType.GetString(bytes);
            }
            catch
            {
                str = result;
            }

            return str;
        }

        
        /// <summary>
        /// 根据base64字符串获取文件后缀（图片格式）
        /// </summary>
        /// <param name="base64Str">base64</param>
        /// <returns></returns>
        public static string GetSuffixFromBase64Str(string base64Str)
        {
            var suffix = string.Empty;
            var prefix = "data:image/";
            if (base64Str.StartsWith(prefix) && base64Str.Contains(";") && base64Str.Contains(","))
            {
                base64Str = base64Str.Split(';')[0];
                suffix = base64Str.Substring(prefix.Length);
            }
            return suffix;
        }

        ///编码
        public static string EncodeBase64(string code_type, string code)
        {
            var encode = "";
            var bytes = Encoding.GetEncoding(code_type).GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }
        ///解码
        public static string DecodeBase64(string code_type, string code)
        {
            var decode = "";
            var bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        }
    }
}
