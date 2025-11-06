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
        // 1. 核心配置：提取常量，明确编码和加密模式（符合AES安全规范）
        private static readonly byte[] _key = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32];
        private static readonly Encoding _defaultEncoding = Encoding.UTF8; // 明确编码，避免系统默认编码差异
        private static readonly int _ivLength = 16; // AES固定IV长度（128位）

        /// <summary>
        /// 加密文本文件（支持中文，自动处理IV）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        /// <exception cref="IOException">文件读写失败时抛出</exception>
        public static void EncryptFile(string filePath)
        {
            // 2. 增强校验：提前判断文件是否存在，明确异常
            if (!File.Exists(filePath))
                throw new FileNotFoundException("待加密的文件不存在", filePath);

            try
            {
                // 读取文件内容（指定编码，避免中文乱码）
                string plainText = File.ReadAllText(filePath, _defaultEncoding);
                // 加密内容（自动生成随机IV）
                string encryptedText = Encrypt(plainText);
                // 写入加密结果（覆盖原文件，指定编码）
                File.WriteAllText(filePath, encryptedText, _defaultEncoding);
            }
            catch (IOException ex)
            {
                throw new IOException("文件加密失败：无法读取或写入文件", ex);
            }
        }

        /// <summary>
        /// 加密字符串（自动生成随机IV，IV与密文一起返回）
        /// </summary>
        /// <param name="plainText">待加密的明文</param>
        /// <returns>加密后的字符串（格式：IV的Base64|密文的Base64）</returns>
        public static string Encrypt(string plainText)
        {
            // 3. 避免重复加密：通过格式判断（IV|密文），而非仅Base64
            if (IsEncrypted(plainText))
                return plainText;
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (Aes aesAlg = CreateAesInstance())
            {
                // 生成随机IV（原代码IV固定为0，严重不安全，此处修复）
                aesAlg.GenerateIV();
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        // 用StreamWriter写入，自动处理编码（匹配_defaultEncoding）
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, _defaultEncoding))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    // 4. 组合IV和密文：IV在前，用|分隔，解密时需拆分（原代码无IV，无法安全解密）
                    string ivBase64 = Convert.ToBase64String(aesAlg.IV);
                    string cipherBase64 = Convert.ToBase64String(msEncrypt.ToArray());
                    return $"{ivBase64}|{cipherBase64}";
                }
            }
        }

        /// <summary>
        /// 解密文本文件（自动提取IV，恢复明文）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>解密后的明文内容</returns>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        public static string DecryptFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("待解密的文件不存在", filePath);

            // 读取加密文件内容（指定编码）
            string cipherText = File.ReadAllText(filePath, _defaultEncoding);
            // 直接调用Decrypt方法，复用逻辑
            return Decrypt(cipherText);
        }

        /// <summary>
        /// 解密密文字符串（自动拆分IV和密文）
        /// </summary>
        /// <param name="cipherText">加密字符串（格式：IV的Base64|密文的Base64）</param>
        /// <returns>解密后的明文</returns>
        /// <exception cref="FormatException">密文格式错误时抛出</exception>
        public static string Decrypt(string cipherText)
        {
            if (!IsEncrypted(cipherText))
                return cipherText;
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            // 5. 拆分IV和密文：按|分割，确保格式正确
            string[] ivAndCipher = cipherText.Split('|');
            if (ivAndCipher.Length != 2)
                throw new FormatException("密文格式错误，正确格式应为「IV的Base64|密文的Base64」");

            using (Aes aesAlg = CreateAesInstance())
            {
                try
                {
                    // 解析IV（必须16字节，AES固定要求）
                    aesAlg.IV = Convert.FromBase64String(ivAndCipher[0]);
                    if (aesAlg.IV.Length != _ivLength)
                        throw new FormatException("IV长度错误，AES要求IV必须为16字节");

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    // 解析密文
                    byte[] cipherBytes = Convert.FromBase64String(ivAndCipher[1]);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            // 用StreamReader读取，匹配加密时的编码
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt, _defaultEncoding))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch (FormatException ex)
                {
                    throw new FormatException("密文解码失败，可能被篡改或格式错误", ex);
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("解密失败，密钥错误或密文损坏", ex);
                }
            }
        }

        /// <summary>
        /// 判断字符串是否已加密（基于「IV|密文」格式+Base64校验）
        /// </summary>
        /// <param name="text">待判断的字符串</param>
        /// <returns>true=已加密，false=未加密</returns>
        public static bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // 6. 更严格的判断：必须包含|，且分割后两部分都是Base64
            string[] ivAndCipher = text.Split('|');
            if (ivAndCipher.Length != 2)
                return false;

            // 校验IV和密文是否都是合法Base64
            return IsValidBase64(ivAndCipher[0]) && IsValidBase64(ivAndCipher[1]);
        }

        #region 私有辅助方法（封装重复逻辑，提高可维护性）
        /// <summary>
        /// 创建AES实例（统一配置，避免重复代码）
        /// </summary>
        private static Aes CreateAesInstance()
        {
            Aes aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC; // 推荐CBC模式（比ECB安全）
            aes.Padding = PaddingMode.PKCS7; // 自动处理非16倍长度数据
            aes.BlockSize = 128; // AES固定块大小，不可修改
            return aes;
        }

        /// <summary>
        /// 校验字符串是否为合法Base64
        /// </summary>
        private static bool IsValidBase64(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length % 4 != 0)
                return false;

            try
            {
                _ = Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}