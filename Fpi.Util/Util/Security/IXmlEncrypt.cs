using System;
using System.Collections.Generic;
using System.Text;
using Fpi.Util.Interfaces;

namespace Fpi.Util.Security
{
    public interface IXmlEncrypt : IEncrypt, INaming
    {
        /// <summary>   
        /// 加密 
        /// </summary>   
        /// <param name="plainText">明文字符串</param>   
        /// <returns>返回加密后的密文字节数组</returns>   
        string Encrypt(string text);

        /// <summary>   
        /// 解密   
        /// </summary>   
        /// <param name="cipherText">密文字节数组</param>   
        /// <returns>返回解密后的字符串</returns>   
        string Decrypt(string text);

        /// <summary>加密标示信息</summary>
        string NodeNameplate
        {
            get;
        }
    }
}
