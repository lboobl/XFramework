using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace XFramework.Core
{
    public class SecurityHelper
    {
        private const string defaultKey = "Yuanyuan";
        private static byte[] keys = Encoding.Default.GetBytes("Yuanyuan"); //Keys长度必须为８位

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString">要解密的字符串</param>
        /// <returns></returns>
        public static string DecryptDes(string decryptString)
        {
            return DecryptDes(decryptString, defaultKey);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString">要解密的字符串</param>
        ///  <param name="decryptKey">密钥，必须为８位?</param>
        /// <returns></returns>
        public static string DecryptDes(string decryptString, string decryptKey)
        {
            //规定密钥只能是数字与字母的组合，并且长度不得小于８位
            Regex regex = new Regex(@"^[0-9a-zA-Z]{8,}");
            if (!regex.IsMatch(decryptKey)) throw new ArgumentOutOfRangeException("decryptKey", "密钥必须由字母或者数字组成，并且长度不得小于８位！");

            try
            {
                using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
                {
                    byte[] inputByte = Convert.FromBase64String(decryptString);
                    byte[] inputKey = Encoding.Default.GetBytes(decryptKey.Substring(0, 8));
                    byte[] inputIV = Encoding.Default.GetBytes(decryptKey);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, provider.CreateDecryptor(keys, inputIV), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByte, 0, inputByte.Length);
                            cs.FlushFinalBlock();
                            return Encoding.Default.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">要加密的字符串</param>
        /// <returns></returns>
        public static string EncryptDes(string encryptString)
        {
            return EncryptDes(encryptString, defaultKey);
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">加密的字符串</param>
        /// <returns></returns>
        public static string EncryptDes(string encryptString, string encryptKey)
        {
            //规定密钥只能是数字与字母的组合，并且长度不得小于８位
            Regex regex = new Regex(@"^[0-9a-zA-Z]{8,}");
            if (!regex.IsMatch(encryptKey)) throw new ArgumentOutOfRangeException("decryptKey", "密钥必须由字母或者数字组成，并且长度不得小于８位！");

            try
            {
                using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
                {
                    byte[] inputByte = Encoding.Default.GetBytes(encryptString);
                    byte[] inputKey = Encoding.Default.GetBytes(encryptKey.Substring(0, 8));
                    byte[] inputIV = Encoding.Default.GetBytes(encryptKey);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, provider.CreateEncryptor(keys, inputIV), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByte, 0, inputByte.Length);
                            cs.FlushFinalBlock();
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }

        /// <summary>
        /// 转为md5加密的字符串
        /// </summary>
        /// <returns></returns>
        public static string Convert2HexMD5(string str, Encoding encoding)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            encoding = encoding ?? Encoding.Default;
            byte[] byteValue = encoding.GetBytes(str);
            byte[] byteHash = md5.ComputeHash(byteValue);
            md5.Clear();

            string value = "";
            for (int i = 0; i < byteHash.Length; i++)
            {
                string hex = Convert.ToString(byteHash[i], 16);
                if (hex.Length < 2) hex = hex.PadLeft(2, '0');
                value += hex;//Convert.ToString(byteHash[i], 16); //bytHash[i].ToString("x2");
            }

            //return value;

            //MD5 m = new MD5CryptoServiceProvider();
            //byte[] b = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            //String output = "";
            //string a = "";
            //for (int i = 0; i < b.Length; i++)
            //{
            //    a = Convert.ToString(b[i], 16);
            //    if (a.Length < 2)
            //    {
            //        a = "0" + a;
            //    }
            //    output = output + a;
            //}
            //return output;

            MD5 m = new MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            StringBuilder ret = new StringBuilder();
            foreach (var c in s)
            {
                string v = Convert.ToString(c, 16);
                if (v.Length == 1)
                {
                    ret.Append("0");
                }
                ret.Append(v);
            }
            return ret.ToString();


        }
    }
}
