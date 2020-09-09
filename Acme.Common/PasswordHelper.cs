using System;


namespace Acme.Common
{
    public static class PasswordHelper
    {
        /// <summary>
        /// 生成随机数的种子
        /// </summary>
        /// <returns></returns>
        private static int GetNewSeed()
        {
            var rndBytes = new byte[4];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(rndBytes);
            return BitConverter.ToInt32(rndBytes, 0);
        }

        /// <summary>
        /// 生成8位随机数
        /// </summary>
        /// <returns></returns>
        public static string GetRandomString(int len)
        {
            const string s = "123456789abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ";
            var reValue = string.Empty;
            var rnd = new Random(GetNewSeed());
            while (reValue.Length < len)
            {
                var s1 = s[rnd.Next(0, s.Length)].ToString();
                if (reValue.IndexOf(s1, StringComparison.Ordinal) == -1) reValue += s1;
            }
            return reValue;
        }
    }
}
