using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Fpi.Util.Security
{
    [Serializable]
    public class AesCryptHelper : IXmlEncrypt
    {
        #region single

        private static object _syncObj = new object();
        private static AesCryptHelper _instance;

        private AesCryptHelper()
        {
        }

        public static AesCryptHelper GetInstance()
        {
            lock (_syncObj)
            {
                if (_instance == null)
                {
                    _instance = new AesCryptHelper();
                }
            }
            return _instance;
        }

        #endregion

        #region body

        //默认密钥向量    
        private static readonly byte[] MY_IV =
        {
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        private const string PublicKey = "fpihz2021.pcdept";

        #endregion

        #region IXmlEncrypt 成员

        /// <summary>   
        /// AES加密算法   
        /// </summary>   
        /// <param name="text">明文字符串</param>   
        /// <returns>返回加密后的密文字节数组</returns>   
        public string Encrypt(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = Encrypt(bytes);
            string res = Convert.ToBase64String(bytes);
            return res;
        }

        /// <summary>   
        /// AES解密   
        /// </summary>   
        /// <param name="text">密文字节数组</param>   
        /// <returns>返回解密后的字符串</returns>   
        public string Decrypt(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);
            bytes = Decrypt(bytes);
            string res = Encoding.UTF8.GetString(bytes);
            //去除尾部自动填充的无效字符
            res = res.TrimEnd('\0');
            return res;
        }

        public string NodeNameplate => "FpiXmlSecurityInfo_A1";

        #endregion

        #region IEncrypt 成员

        public byte[] Encrypt(byte[] data)
        {
            //分组加密算法   
            SymmetricAlgorithm des = Rijndael.Create();

            //设置密钥及密钥向量   
            des.Key = Encoding.UTF8.GetBytes(PublicKey);
            des.IV = MY_IV;

            byte[] cipherBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组   
                    //cs.Close();
                }
                //ms.Close();
            }
            return cipherBytes;
        }

        public byte[] Decrypt(byte[] data)
        {
            //分组加密算法   
            SymmetricAlgorithm des = Rijndael.Create();

            //设置密钥及密钥向量   
            des.Key = Encoding.UTF8.GetBytes(PublicKey);
            des.IV = MY_IV;

            byte[] decryptBytes = new byte[data.Length];
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    //cs.Close();
                }
                //ms.Close();
            }
            return decryptBytes;
        }

        #endregion

        #region INaming 成员

        public string FriendlyName
        {
            get { return "AES"; }
        }

        #endregion
    }
}
