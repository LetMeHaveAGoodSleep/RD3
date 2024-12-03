using System.Text;
using System;
using Fpi.Properties;
namespace Fpi.Communication.Converter
{
    public class BracketDataConverter : IDataConvertable
    {
        public BracketDataConverter()
        {
        }

        private static object syncObj = new object();
        private static BracketDataConverter instance = null;

        public static IDataConvertable GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new BracketDataConverter();
                }
            }
            return instance;
        }

        #region const define

        private const double MILLION_E_6 = 1e-6;
        private const double MILLION_E_12 = 1e-12;
        private const double MILLION_E_18 = 1e-18;
        private const double MILLION_E_24 = 1e-24;
        private const double MILLION_E_30 = 1e-30;
        private const double MILLION_E_31 = 1e-31;
        private const double MILLION_E_32 = 1e-32;

        private const double MILLION_E6 = 1e6;
        private const double MILLION_E12 = 1e12;
        private const double MILLION_E18 = 1e18;
        private const double MILLION_E24 = 1e24;
        private const double MILLION_E30 = 1e30;
        private const double MILLION_E31 = 1e31;
        private const double MILLION_E32 = 1e32;
        private const double MILLION_E36 = 1e36;

        #endregion

        public int GetTypeLength(string typeName)
        {
            switch (typeName)
            {
                case "bit":
                    return -1;
                case "string":
                    return 1;
                case "byte":
                    return 1;
                case "uint":
                    return 2;
                case "ulong":
                    return 4;
                case "int":
                    return 2;
                case "long":
                    return 4;
                case "float":
                    return 4;
                default:
                    throw new NotSupportedException(typeName + " not suported in bracket data converter。");
            }
        }


        public byte[] GetBytes(string value)
        {
            return ASCIIEncoding.ASCII.GetBytes(value);
        }

        //说明：自定义的浮点数存储格式
        //原理：依据浮点数的大小，分范围分别处理
        public byte[] GetBytes(float value)
        {
            byte[] bytes = new byte[4];
            sbyte ch_ExpSign;
            byte unch_DataSign;
            ulong ul_OutPut;
            float f2;
            float f3;
            f2 = value;
            unch_DataSign = 0;
            ch_ExpSign = 0;
            if ((value > (-1e-31)) && (value < 1e-31)) //whether value==0;
            {
                bytes[0] = 0;
                bytes[1] = 0;
                bytes[2] = 0;
                bytes[3] = 0;
                return bytes;
            }

            if ((value <= (-9.999995e31)) || (value >= 9.999995e31)) //whether value==0;
            {
                bytes[0] = 0xff;
                bytes[1] = 0xff;
                bytes[2] = 0xff;
                bytes[3] = 0xff;
                return bytes;
            }
            if (value < 0.0) // whether f1，0
            {
                f2 = -value;
                unch_DataSign = 0x80;
            }

            f3 = f2;

            if (f3 < 1)
            {
                if (f3 >= MILLION_E_6)
                {
                    if (f3 >= 1e-3)
                    {
                        if (f3 >= 1e-1)
                        {
                            ch_ExpSign = -1;
                            ul_OutPut = (ulong) (f3*1e6 + 0.5);
                        }
                        else if (f3 >= 1e-2)
                        {
                            ch_ExpSign = -2;
                            ul_OutPut = (ulong) (f3*1e7 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -3;
                            ul_OutPut = (ulong) (f3*1e8 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 >= 1e-4)
                        {
                            ch_ExpSign = -4;
                            ul_OutPut = (ulong) (f3*1e9 + 0.5);
                        }
                        else if (f3 >= 1e-5)
                        {
                            ch_ExpSign = -5;
                            ul_OutPut = (ulong) (f3*1e10 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -6;
                            ul_OutPut = (ulong) (f3*1e11 + 0.5);
                        }
                    }
                }
                else if (f3 >= MILLION_E_12)
                {
                    if (f3 >= 1e-9)
                    {
                        if (f3 >= 1e-7)
                        {
                            ch_ExpSign = -7;
                            ul_OutPut = (ulong) (f3*1e12 + 0.5);
                        }
                        else if (f3 >= 1e-8)
                        {
                            ch_ExpSign = -8;
                            ul_OutPut = (ulong) (f3*1e13 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -9;
                            ul_OutPut = (ulong) (f3*1e14 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 >= 1e-10)
                        {
                            ch_ExpSign = -10;
                            ul_OutPut = (ulong) (f3*1e15 + 0.5);
                        }
                        else if (f3 >= 1e-11)
                        {
                            ch_ExpSign = -11;
                            ul_OutPut = (ulong) (f3*1e16 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -12;
                            ul_OutPut = (ulong) (f3*1e17 + 0.5);
                        }
                    }
                }
                else if (f3 >= MILLION_E_18)
                {
                    if (f3 >= 1e-15)
                    {
                        if (f3 >= 1e-13)
                        {
                            ch_ExpSign = -13;
                            ul_OutPut = (ulong) (f3*1e18 + 0.5);
                        }
                        else if (f3 >= 1e-14)
                        {
                            ch_ExpSign = -14;
                            ul_OutPut = (ulong) (f3*1e19 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -15;
                            ul_OutPut = (ulong) (f3*1e20 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 >= 1e-16)
                        {
                            ch_ExpSign = -16;
                            ul_OutPut = (ulong) (f3*1e21 + 0.5);
                        }
                        else if (f3 >= 1e-17)
                        {
                            ch_ExpSign = -17;
                            ul_OutPut = (ulong) (f3*1e22 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -18;
                            ul_OutPut = (ulong) (f3*1e23 + 0.5);
                        }
                    }
                }

                else if (f3 >= MILLION_E_24)
                {
                    if (f3 >= 1e-21)
                    {
                        if (f3 >= 1e-19)
                        {
                            ch_ExpSign = -19;
                            ul_OutPut = (ulong) (f3*1e24 + 0.5);
                        }
                        else if (f3 >= 1e-20)
                        {
                            ch_ExpSign = -20;
                            ul_OutPut = (ulong) (f3*1e25 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -21;
                            ul_OutPut = (ulong) (f3*1e26 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 >= 1e-22)
                        {
                            ch_ExpSign = -22;
                            ul_OutPut = (ulong) (f3*1e27 + 0.5);
                        }
                        else if (f3 >= 1e-23)
                        {
                            ch_ExpSign = -23;
                            ul_OutPut = (ulong) (f3*1e28 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -24;
                            ul_OutPut = (ulong) (f3*1e29 + 0.5);
                        }
                    }
                }
                else if (f3 >= MILLION_E_30)
                {
                    if (f3 >= 1e-27)
                    {
                        if (f3 >= 1e-25)
                        {
                            ch_ExpSign = -25;
                            ul_OutPut = (ulong) (f3*1e30 + 0.5);
                        }
                        else if (f3 >= 1e-26)
                        {
                            ch_ExpSign = -26;
                            ul_OutPut = (ulong) (f3*1e31 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -27;
                            ul_OutPut = (ulong) (f3*1e32 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 >= 1e-28)
                        {
                            ch_ExpSign = -28;
                            ul_OutPut = (ulong) (f3*1e33 + 0.5);
                        }
                        else if (f3 >= 1e-29)
                        {
                            ch_ExpSign = -29;
                            ul_OutPut = (ulong) (f3*1e34 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = -30;
                            ul_OutPut = (ulong) (f3*1e35 + 0.5);
                        }
                    }
                }
                else
                {
                    ch_ExpSign = -31;
                    ul_OutPut = (ulong) (f3*1e36 + 0.5);
                }
            }

            else
            {
                if (f3 < MILLION_E6)
                {
                    if (f3 < 1e3)
                    {
                        if (f3 < 1e1)
                        {
                            ch_ExpSign = 0;
                            ul_OutPut = (ulong) (f3*1e5 + 0.5);
                        }
                        else if (f3 < 1e2)
                        {
                            ch_ExpSign = 1;
                            ul_OutPut = (ulong) (f3*1e4 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 2;
                            ul_OutPut = (ulong) (f3*1e3 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 < 1e4)
                        {
                            ch_ExpSign = 3;
                            ul_OutPut = (ulong) (f3*1e2 + 0.5);
                        }
                        else if (f3 < 1e5)
                        {
                            ch_ExpSign = 4;
                            ul_OutPut = (ulong) (f3*1e1 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 5; //f3=f3*0;
                            ul_OutPut = (ulong) (f3 + 0.5);
                        }
                    }
                }
                else if (f3 < MILLION_E12)
                {
                    if (f3 < 1e9)
                    {
                        if (f3 < 1e7)
                        {
                            ch_ExpSign = 6;
                            ul_OutPut = (ulong) (f3*1e-1 + 0.5);
                        }
                        else if (f3 < 1e8)
                        {
                            ch_ExpSign = 7;
                            ul_OutPut = (ulong) (f3*1e-2 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 8;
                            ul_OutPut = (ulong) (f3*1e-3 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 < 1e10)
                        {
                            ch_ExpSign = 9;
                            ul_OutPut = (ulong) (f3*1e-4 + 0.5);
                        }
                        else if (f3 < 1e11)
                        {
                            ch_ExpSign = 10;
                            ul_OutPut = (ulong) (f3*1e-5 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 11;
                            ul_OutPut = (ulong) (f3*1e-6 + 0.5);
                        }
                    }
                }
                else if (f3 < MILLION_E18)
                {
                    if (f3 < 1e15)
                    {
                        if (f3 < 1e13)
                        {
                            ch_ExpSign = 12;
                            ul_OutPut = (ulong) (f3*1e-7 + 0.5);
                        }
                        else if (f3 < 1e14)
                        {
                            ch_ExpSign = 13;
                            ul_OutPut = (ulong) (f3*1e-8 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 14;
                            ul_OutPut = (ulong) (f3*1e-9 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 < 1e16)
                        {
                            ch_ExpSign = 15;
                            ul_OutPut = (ulong) (f3*1e-10 + 0.5);
                        }
                        else if (f3 < 1e17)
                        {
                            ch_ExpSign = 16;
                            ul_OutPut = (ulong) (f3*1e-11 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 17;
                            ul_OutPut = (ulong) (f3*1e-12 + 0.5);
                        }
                    }
                }

                else if (f3 < MILLION_E24)
                {
                    if (f3 < 1e21)
                    {
                        if (f3 < 1e19)
                        {
                            ch_ExpSign = 18;
                            ul_OutPut = (ulong) (f3*1e-13 + 0.5);
                        }
                        else if (f3 < 1e20)
                        {
                            ch_ExpSign = 19;
                            ul_OutPut = (ulong) (f3*1e-14 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 20;
                            ul_OutPut = (ulong) (f3*1e-15 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 < 1e22)
                        {
                            ch_ExpSign = 21;
                            ul_OutPut = (ulong) (f3*1e-16 + 0.5);
                        }
                        else if (f3 < 1e23)
                        {
                            ch_ExpSign = 22;
                            ul_OutPut = (ulong) (f3*1e-17 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 23;
                            ul_OutPut = (ulong) (f3*1e-18 + 0.5);
                        }
                    }
                }
                else if (f3 < MILLION_E30)
                {
                    if (f3 < 1e27)
                    {
                        if (f3 < 1e25)
                        {
                            ch_ExpSign = 24;
                            ul_OutPut = (ulong) (f3*1e-19 + 0.5);
                        }
                        else if (f3 < 1e26)
                        {
                            ch_ExpSign = 25;
                            ul_OutPut = (ulong) (f3*1e-20 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 26;
                            ul_OutPut = (ulong) (f3*1e-21 + 0.5);
                        }
                    }
                    else

                    {
                        if (f3 < 1e28)
                        {
                            ch_ExpSign = 27;
                            ul_OutPut = (ulong) (f3*1e-22 + 0.5);
                        }
                        else if (f3 < 1e29)
                        {
                            ch_ExpSign = 28;
                            ul_OutPut = (ulong) (f3*1e-23 + 0.5);
                        }
                        else
                        {
                            ch_ExpSign = 29;
                            ul_OutPut = (ulong) (f3*1e-24 + 0.5);
                        }
                    }
                }
                else if (f3 < MILLION_E31)
                {
                    ch_ExpSign = 30;
                    ul_OutPut = (ulong) (f3*1e-25 + 0.5);
                }

                else
                {
                    ch_ExpSign = 31;
                    ul_OutPut = (ulong) (f3*1e-26 + 0.5);
                }
            }


            if (ul_OutPut > 999999)
            {
                ul_OutPut = ul_OutPut/10;
                ch_ExpSign = (sbyte) (ch_ExpSign + 1);
            }
            ch_ExpSign = (sbyte) (ch_ExpSign + 1); //与原来的老程序的浮点数格式保持一致。
            bytes[2] = (byte) (ul_OutPut%100);
            ul_OutPut = ul_OutPut/100;
            bytes[1] = (byte) (ul_OutPut%100);
            ul_OutPut = ul_OutPut/100;
            bytes[0] = (byte) (ul_OutPut%100);

            if (ch_ExpSign >= 0)
            {
                bytes[3] = (byte) (unch_DataSign | (byte) ch_ExpSign);
            }
            else
            {
                ch_ExpSign = (sbyte) (-ch_ExpSign);
                bytes[3] = (byte) (unch_DataSign | (byte) ch_ExpSign | 0x40);
            }

            bytes[3] = (byte) (bytes[3] & 0xdf);
            return bytes;
        }


        //说明：自定义的无符号整型数存储格式，该整型数的范围是0~9999
        public byte[] GetBytes(uint value)
        {
            if ((value < 0) || (value > 9999))
            {
                throw(new OverflowException(Resources.IntScope));
            }
            byte[] bytes = new byte[2];
            bytes[0] = (byte) (value/100);
            bytes[1] = (byte) (value%100);
            return bytes;
        }


        //说明：自定义的无符号长整型数存储格式，该长整型数的范围是0~99999999
        public byte[] GetBytes(ulong value)
        {
            if ((value < 0) || (value > 99999999))
            {
                throw(new OverflowException(Resources.LongScope));
            }
            byte[] bytes = new byte[4];
            bytes[0] = (byte) (value/1000000);
            bytes[1] = (byte) ((value%1000000)/10000);
            bytes[2] = (byte) ((value%10000)/100);
            bytes[3] = (byte) (value%100);
            return bytes;
        }


        public byte[] GetBytes(int value)
        {
            return GetBytes((uint) value);
            ;
        }

        public byte[] GetBytes(long value)
        {
            return GetBytes((ulong) value);
            ;
        }


        public string ToString(byte[] value, int startIndex, int length)
        {
            for (int i = startIndex + length - 1; i >= startIndex; i--)
            {
                if (value[i] != (byte) 0)
                {
                    length = i + 1 - startIndex;
                    break;
                }
            }
            return UTF8Encoding.UTF8.GetString(value, startIndex, length);
        }

        public float ToSingle(byte[] value, int startIndex)
        {
            float f1;
            uint uni_i;
            byte n;
            byte uch_DataSign;
            byte uch_Data;
            sbyte ch_ExpSign;
            uch_DataSign = 0;
            ch_ExpSign = 1;
            if (value[startIndex] != 0xff)
            {
                if ((value[startIndex + 3] & 0x80) == 0x80)
                    uch_DataSign = 10;
                if ((value[startIndex + 3] & 0x40) == 0x40)
                    ch_ExpSign = -1;
                f1 = (float) 0.0;
                for (n = 0; n < 3; n++)
                {
                    uni_i = (uint) value[startIndex + n];
                    f1 = f1*100 + (float) uni_i;
                }
                f1 = f1/1000000;
                uch_Data = (byte) (value[startIndex + 3] & 0x3f);
                if (ch_ExpSign > 0)
                {
                    for (n = 0; n < uch_Data; n++)
                        f1 = f1*10;
                }
                if (ch_ExpSign < 0)
                {
                    for (n = 0; n < uch_Data; n++)
                        f1 = f1/10;
                }
                if (uch_DataSign == 10)
                    f1 = -f1;
            }
            else f1 = -1000;
            return f1;
        }

        public uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint) (value[startIndex]*100 + value[startIndex + 1]);
        }

        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong) (value[startIndex]*1000000 + value[startIndex + 1]*10000
                            + value[startIndex + 2]*100 + value[startIndex + 3]);
        }

        public int ToInt32(byte[] value, int startIndex)
        {
            return (int) ToUInt32(value, startIndex);
        }

        public long ToInt64(byte[] value, int startIndex)
        {
            return (long) ToUInt64(value, startIndex);
        }
    }
}