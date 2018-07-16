using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace XFramework.Core
{
    /// <summary>
    /// 字符串加解密助手类
    /// </summary>
    public class CryptHepler
    {
        #region 哈希加密

        /*
         * 哈希函数将任意长度的二进制字符串映射为固定长度的小型二进制字符串。
         * 加密哈希函数有这样一个属性：在计算上不大可能找到散列为相同的值的两个不同的输入；也就是说，两组数据的哈希值仅在对应的数据也匹配时才会匹配。数据的少量更改会在哈希值中产生不可预知的大量更改。
         * MD5 算法的哈希值大小为 128 位。
         * MD5 类的 ComputeHash 方法将哈希作为 16 字节的数组返回。请注意，某些 MD5 实现会生成 32 字符的十六进制格式哈希。若要与此类实现进行互操作，请将 ComputeHash 方法的返回值格式化为十六进制值。
         */

       /// <summary>
        /// 取输入字符串的MD5散列值
       /// </summary>
       /// <param name="input">输入字符</param>
       /// <returns></returns>
        public static string MD5(string input)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) builder.Append(data[i].ToString("x2"));
            return builder.ToString();
        }

        /// <summary>
        /// 取输入字符串的SHA1散列值
        /// </summary>
        /// <param name="input">输入字符</param>
        /// <returns></returns>
        public static string SHA1(string input)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) builder.Append(data[i].ToString("x2"));
            return builder.ToString();
        }

        /// <summary>
        /// 获取输入流的由MD5计算的Hash值，不可逆转
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <returns></returns>
        public static string MD5(Stream inputStream)
        {
            var md5 = System.Security.Cryptography.MD5.Create();//不可逆转
            byte[] data = md5.ComputeHash(inputStream);
            var builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) builder.Append(data[i].ToString("x2"));//十六进
            // 返回十六进制的字符串
            return builder.ToString();
        }

        /// <summary>
        /// 获取输入流的由SHA1计算的Hash值，不可逆转
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <returns></returns>
        public static string SHA1(Stream inputStream)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            byte[] data = sha1.ComputeHash(inputStream);
            var builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) builder.Append(data[i].ToString("x2"));//十六进
            // 返回十六进制的字符串
            return builder.ToString();
        }

        /// <summary>
        /// 验证输入流由MD5计算的Hash值
        /// </summary>
        /// <param name="inputStream">输入流</param>
        /// <param name="hash">哈希值</param>
        /// <returns></returns>
        public static bool VerifyMD5Hash(Stream inputStream, string hash)
        {
            string hashOfInputStream = MD5(inputStream);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInputStream, hash) == 0;
        }

        #endregion

        #region DES 加解密

        private const string KEY = "YUANYUAN"; //DES加密密钥和向量
        //private const string IV  = "3dd3d3644d475160fd192a9e60d73647";  //DES加密向量

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="input">要解密的字符串</param>
        ///  <param name="decryptKey">密钥，必须为８位?</param>
        /// <returns></returns>
        public static string DecryptDes(string input)
        {
            using (var provider = new DESCryptoServiceProvider())
            {
                byte[] data = Convert.FromBase64String(input); 
                byte[] ivKey = Encoding.UTF8.GetBytes(KEY);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, provider.CreateDecryptor(ivKey, ivKey), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="input">加密的字符串</param>
        /// <returns></returns>
        public static string EncryptDes(string input)
        {
            using (var provider = new DESCryptoServiceProvider())
            {
                byte[] data = Encoding.UTF8.GetBytes(input);
                byte[] ivKey = Encoding.UTF8.GetBytes(KEY);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, provider.CreateEncryptor(ivKey, ivKey), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        #endregion
    }
}
