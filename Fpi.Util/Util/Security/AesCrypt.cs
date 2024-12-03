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

        //Ĭ����Կ����    
        private static readonly byte[] MY_IV =
        {
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF,
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        private const string PublicKey = "fpihz2021.pcdept";

        #endregion

        #region IXmlEncrypt ��Ա

        /// <summary>   
        /// AES�����㷨   
        /// </summary>   
        /// <param name="text">�����ַ���</param>   
        /// <returns>���ؼ��ܺ�������ֽ�����</returns>   
        public string Encrypt(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            bytes = Encrypt(bytes);
            string res = Convert.ToBase64String(bytes);
            return res;
        }

        /// <summary>   
        /// AES����   
        /// </summary>   
        /// <param name="text">�����ֽ�����</param>   
        /// <returns>���ؽ��ܺ���ַ���</returns>   
        public string Decrypt(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);
            bytes = Decrypt(bytes);
            string res = Encoding.UTF8.GetString(bytes);
            //ȥ��β���Զ�������Ч�ַ�
            res = res.TrimEnd('\0');
            return res;
        }

        public string NodeNameplate => "FpiXmlSecurityInfo_A1";

        #endregion

        #region IEncrypt ��Ա

        public byte[] Encrypt(byte[] data)
        {
            //��������㷨   
            SymmetricAlgorithm des = Rijndael.Create();

            //������Կ����Կ����   
            des.Key = Encoding.UTF8.GetBytes(PublicKey);
            des.IV = MY_IV;

            byte[] cipherBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//�õ����ܺ���ֽ�����   
                    //cs.Close();
                }
                //ms.Close();
            }
            return cipherBytes;
        }

        public byte[] Decrypt(byte[] data)
        {
            //��������㷨   
            SymmetricAlgorithm des = Rijndael.Create();

            //������Կ����Կ����   
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

        #region INaming ��Ա

        public string FriendlyName
        {
            get { return "AES"; }
        }

        #endregion
    }
}
