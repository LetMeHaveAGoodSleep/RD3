using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AESEncryption
    {
        static readonly byte[] key = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };

        public static void EncryptFile(string filePath)
        {
          string plainText=  File.ReadAllText(filePath);
            plainText = Encrypt(plainText);
            File.WriteAllText(filePath, plainText);
        }

        public static string Encrypt(string plainText)
        {
            if (IsEncrypt(plainText)) { return plainText; }
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = new byte[aesAlg.BlockSize / 8];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string DecryptFile(string filePath)
        {
            string cipherText = File.ReadAllText(filePath);
            if (!IsEncrypt(cipherText))
            {
                return cipherText;
            }
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = new byte[aesAlg.BlockSize / 8];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static bool IsEncrypt(string plainText)
        {
            bool res = false;
            // 如果字符串为空，或者长度不是4的倍数，则不是有效的Base64编码  
            if (string.IsNullOrEmpty(plainText) || plainText.Length % 4 != 0)
            {
                return false;
            }

            try
            {
                // 尝试将字符串解码为字节数组  
                byte[] decodedBytes = Convert.FromBase64String(plainText);
                // 如果解码成功，则说明字符串是有效的Base64编码  
                return true;
            }
            catch (FormatException ex)
            {
                // 如果解码失败（抛出FormatException异常），则说明字符串不是有效的Base64编码  
                return false;
            }
        }
    }
}
