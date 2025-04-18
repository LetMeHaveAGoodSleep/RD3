using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Security
{
    public interface IEncrypt
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
