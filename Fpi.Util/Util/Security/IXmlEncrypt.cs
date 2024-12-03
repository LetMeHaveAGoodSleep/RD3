using System;
using System.Collections.Generic;
using System.Text;
using Fpi.Util.Interfaces;

namespace Fpi.Util.Security
{
    public interface IXmlEncrypt : IEncrypt, INaming
    {
        /// <summary>   
        /// ���� 
        /// </summary>   
        /// <param name="plainText">�����ַ���</param>   
        /// <returns>���ؼ��ܺ�������ֽ�����</returns>   
        string Encrypt(string text);

        /// <summary>   
        /// ����   
        /// </summary>   
        /// <param name="cipherText">�����ֽ�����</param>   
        /// <returns>���ؽ��ܺ���ַ���</returns>   
        string Decrypt(string text);

        /// <summary>���ܱ�ʾ��Ϣ</summary>
        string NodeNameplate
        {
            get;
        }
    }
}
