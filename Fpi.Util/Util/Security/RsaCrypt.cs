using System;
using System.Security.Cryptography;
using System.Text;
using Fpi.Util.Interfaces;

namespace Fpi.Util.Security
{
    public class RsaCryptHelper : IEncrypt, INaming
    {
        private static object syncObj = new object();
        private static RsaCryptHelper instance = null;
        private RsaCryptHelper()
        { }
        public static RsaCryptHelper GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new RsaCryptHelper();
                }
            }
            return instance;
        }

        private const String publicKey =
            "<RSAKeyValue><Modulus>u+bmnTgtCB5D5QR9VVOfH43ymBttYIoYkw6gdQr/uIuc3CPdMqmAms5ye1RTevilNsUaqMO6heTCglC9VFhssvAD8vbAkS+lDWvDI9TtL+MH7SwBpVAqOxE1JedVQZA8SQrDdFmHQ9pZ4PkLYIf5/mGNtazpat+bTOg0PkK0nNs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const String privateKey =
            "<RSAKeyValue><Modulus>u+bmnTgtCB5D5QR9VVOfH43ymBttYIoYkw6gdQr/uIuc3CPdMqmAms5ye1RTevilNsUaqMO6heTCglC9VFhssvAD8vbAkS+lDWvDI9TtL+MH7SwBpVAqOxE1JedVQZA8SQrDdFmHQ9pZ4PkLYIf5/mGNtazpat+bTOg0PkK0nNs=</Modulus><Exponent>AQAB</Exponent><P>9rM+nj8PzjwFwaKgiolCvo4ubl8RPWUecjVL8vehUpi8zoNuyX76CvZcmwiZlEt8NM1K4xzuPMdP+941z0o/ew==</P><Q>wvw62wEhDswp0LeZEDUFnUKm3v6bXhZay1oSDxFjJW9r6Ei9j/P17USrH3dQE1xsfSSo9lhc6pttu/vCG5TqIQ==</Q><DP>Z2euYYoxR9Kk3wsZm7f5AAJ8t8qlYUSXRGvOj+L3/bUDvtQchdzxVdL58gnixeP2BfPe5d9khJlOj1YO2/pVLQ==</DP><DQ>cYDuqeogWkLS3KLjwSF8YS0Zgpnny39r3xBRjt/qPTJ9ODyPzKqRMEtW6fxEauDUbozWpoCNpixQVquZNQEcwQ==</DQ><InverseQ>7oyFwfaEyW4DNDKDUeptmCuDgppP5m+wF0IPdspn2FrKYbRxxk+81b10k9kam/3WR6ntbr2nbbbHjrQe3yekqA==</InverseQ><D>Ph8pkUL4SF3RLo8cbLBXxFPmp3kx4R/m3f+Q9wEq6DVoJ7PHtq8pcITefn2zl81Kud/SX4dOTGQEFpdg8NjHHpOKvBvM7IuDhykQsZeI2Ezw3vhhl6EMwdIaVgst+GjfGu0HrQsxrUXt0v8Flsi0+qxzP1KMasw6DsT6o5T4YcE=</D></RSAKeyValue>";

        public string Encryt(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);//Encoding.GetEncoding("gb2312").GetBytes(text);            
            bytes = Encrypt(bytes);
            string res = Convert.ToBase64String(bytes);
            return res;
        }

        public string Decrypt(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);
            bytes = Decrypt(bytes);
            string res = Encoding.UTF8.GetString(bytes);//Encoding.GetEncoding("gb2312").GetString(bytes);
            return res;
        }

        #region IEncrypt 成员

        public byte[] Encrypt(byte[] data)
        {
#if !WINCE

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            return rsa.Encrypt(data, false);
#else
			return data;
#endif
        }

        public byte[] Decrypt(byte[] data)
        {
#if !WINCE

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            return rsa.Decrypt(data, false);
#else
			return data;
#endif
        }

        #endregion

        #region INaming 成员

        public string FriendlyName
        {
            get { return "RSA"; }
        }

        #endregion
    }
}