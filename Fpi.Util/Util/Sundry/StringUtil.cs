using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// StringParser 的摘要说明。
    /// </summary>
    public class StringUtil
    {
        private StringUtil()
        {
        }

        public static byte ParseByte(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return Byte.Parse(value.Substring(2), NumberStyles.HexNumber);
            return Byte.Parse(value);
        }
        public static short ParseShort(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return Int16.Parse(value.Substring(2), System.Globalization.NumberStyles.HexNumber);
            return Int16.Parse(value);
        }
        public static ushort ParseUShort(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return UInt16.Parse(value.Substring(2), System.Globalization.NumberStyles.HexNumber);
            return UInt16.Parse(value);
        }
        public static int ParseInt(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return Int32.Parse(value.Substring(2), NumberStyles.HexNumber);
            return Int32.Parse(value);
        }

        public static uint ParseUInt(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return UInt32.Parse(value.Substring(2), NumberStyles.HexNumber);
            return UInt32.Parse(value);
        }

        public static long ParseLong(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return Int64.Parse(value.Substring(2), NumberStyles.HexNumber);
            return Int64.Parse(value);
        }

        public static ulong ParseULong(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return UInt64.Parse(value.Substring(2), NumberStyles.HexNumber);
            return UInt64.Parse(value);
        }

        /// <summary>
        /// 只有用逗号分隔的字符串才可调用此函数.
        /// </summary>
        public static string[] ParseStrings(string value)
        {
            if (value == null)
            {
                return new string[0];
            }
            value = value.Replace(";", ",");
            string[] strValues = value.Split(',');

            if (strValues != null)
            {
                for (int i = 0; i < strValues.Length; i++)
                {
                    strValues[i] = strValues[i].Trim();
                }
                return strValues;
            }
            return new string[0];
        }

        public static string ByteToString(byte value)
        {
            return value.ToString("X2");
        }

        public static string UIntToString(uint value)
        {
            return value.ToString();
        }

        public static string ULongToString(ulong value)
        {
            return value.ToString();
        }

        public static string IntToString(int value)
        {
            return value.ToString();
        }

        public static string LongToString(long value)
        {
            return value.ToString();
        }

        public static string FloatToString(float value)
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            return value.ToString("0.##");
        }

        public static string BytesToString(byte[] data)
        {
            return BytesToString(data, data.Length);
        }

        public static string BytesToString(byte[] data, int count)
        {
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                string s = ByteToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        public static string UIntsToString(uint[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string s = UIntToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        public static string ULongsToString(ulong[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string s = ULongToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        public static string IntsToString(int[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string s = IntToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        public static string LongsToString(long[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string s = LongToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        public static string FloatsToString(float[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string s = FloatToString(data[i]);
                sb.Append(s).Append(" ");
            }
            return sb.ToString();
        }

        private static byte ParseByteHex(string value)
        {
            if ((value != null) && value.ToLower().StartsWith("0x"))
                return Byte.Parse(value.Substring(2), NumberStyles.HexNumber);
            return Byte.Parse(value, NumberStyles.HexNumber);
        }

        /// <summary>
        /// 只有用空格分隔的字符串才可调用此函数.
        /// </summary>
        public static byte[] ParseBytes(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new byte[0];
            string[] strValues = value.Split(' ');

            byte[] tempValues = new byte[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = ParseByteHex(strValues[i].Trim());
                    count++;
                }
                catch (Exception)
                {
                }
            }
            byte[] values = new byte[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count);
            return values;
        }

        public static int[] ParseInts(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new int[0];
            string[] strValues = value.Split(" ,".ToCharArray());
            int[] tempValues = new int[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = ParseInt(strValues[i].Trim());
                    count++;
                }
                catch (Exception)
                {
                }
            }
            int[] values = new int[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count*Marshal.SizeOf(Int32.MaxValue));
            return values;
        }

        public static uint[] ParseUInts(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new uint[0];
            string[] strValues = value.Split(" ,".ToCharArray());
            uint[] tempValues = new uint[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = ParseUInt(strValues[i].Trim());
                    count++;
                }
                catch (Exception)
                {
                }
            }
            uint[] values = new uint[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count*Marshal.SizeOf(UInt32.MaxValue));
            return values;
        }

        public static long[] ParseLongs(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new long[0];
            string[] strValues = value.Split(" ,".ToCharArray());
            long[] tempValues = new long[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = ParseLong(strValues[i].Trim());
                    count++;
                }
                catch (Exception)
                {
                }
            }
            long[] values = new long[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count*Marshal.SizeOf(Int64.MaxValue));
            return values;
        }

        public static ulong[] ParseULongs(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new ulong[0];
            string[] strValues = value.Split(" ,".ToCharArray());
            ulong[] tempValues = new ulong[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = ParseULong(strValues[i].Trim());
                    count++;
                }
                catch (Exception)
                {
                }
            }
            ulong[] values = new ulong[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count*Marshal.SizeOf(UInt64.MaxValue));
            return values;
        }

        public static float[] ParseFloats(string value)
        {
            if ((value == null) || (value.Trim().Length <= 0))
                return new float[0];
            string[] strValues = value.Split(" ,".ToCharArray());
            float[] tempValues = new float[strValues.Length];
            int count = 0;
            for (int i = 0; i < tempValues.Length; i++)
            {
                try
                {
                    tempValues[count] = Single.Parse(strValues[i].Trim()); //有点不同
                    count++;
                }
                catch (Exception)
                {
                }
            }
            float[] values = new float[count];
            Buffer.BlockCopy(tempValues, 0, values, 0, count*Marshal.SizeOf(Single.MaxValue));
            return values;
        }

        #region 实用方法


        #region 取得汉字字符串的拼音的首字母集合字符串
        /// <summary>
        /// 取得汉字字符串的拼音的首字母集合字符串
        /// </summary>
        public static string GetChineseSpell(string strText)
        {
            int len = strText.Length;
            string myStr = "";
            for (int i = 0; i < len; i++)
            {
                myStr += GetSpell(strText.Substring(i, 1));
            }
            return myStr;
        }

        #endregion

        #region 取汉字的首字母

        /// <summary>
        /// 取汉字的首字母
        /// </summary>
        /// <param name="chinaString">输入的汉字字符串</param>
        /// <returns></returns>
        public static string GetSpell(string chinaString)
        {
            byte[] arrCN = Encoding.Default.GetBytes(chinaString);
            if (arrCN.Length > 1)
            {
                int area = (short)arrCN[0];
                int pos = (short)arrCN[1];
                int code = (area << 8) + pos;
                int[] areacode = { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481 };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25) max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                    {
                        return Encoding.Default.GetString(new byte[] { (byte)(65 + i) });
                    }
                }
                return "*";
            }
            else
                return chinaString;
        }

        #endregion

        #region 转为大写人民币
        ///// <summary>
        ///// 转为大写人民币
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns></returns>
        //public static string ToRMB(string strSource)
        //{
        //    string x = strSource;
        //    string ret = "";
        //    int nnum;
        //    x = x.Replace("-", "");
        //    if (x.IndexOf(".") > -1)
        //    {
        //        if (x.Length == (x.IndexOf(".") + 2)) x = x + "0";
        //        nnum = int.Parse(x.Substring(0, x.IndexOf(".")));
        //        if (x.Substring(x.IndexOf(".") + 1, 2) == "00")
        //            ret = ToInt(nnum.ToString()) + "元整";
        //        else ret = ToInt(nnum.ToString()) + "元" + ToDot(x.Substring(x.IndexOf(".") + 1, 2));
        //    }
        //    else
        //    {
        //        ret = ToInt(x) + "元整";
        //    }
        //    return ret;
        //}

        //#region 私有方法
        //private static char ToNum(char x)
        //{
        //    string strChnNames = "零壹贰叁肆伍陆柒捌玖";
        //    string strNumNames = "0123456789";
        //    return strChnNames[strNumNames.IndexOf(x)];
        //}
        //private static string ChangeInt(string x)
        //{
        //    string[] strArrayLevelNames = new string[4] { "", "拾", "佰", "仟" };
        //    string ret = "";
        //    int i;
        //    for (i = x.Length - 1; i >= 0; i--)
        //        if (x[i] == '0')
        //            ret = ToNum(x[i]) + ret;
        //        else
        //            ret = ToNum(x[i]) + strArrayLevelNames[x.Length - 1 - i] + ret;
        //    while ((i = ret.IndexOf("零零")) != -1)
        //        ret = ret.Remove(i, 1);
        //    if (ret[ret.Length - 1] == '零' && ret.Length > 1)
        //        ret = ret.Remove(ret.Length - 1, 1);
        //    if (ret.Length >= 2 && ret.Substring(0, 2) == "壹拾")
        //        ret = ret.Remove(0, 1);
        //    return ret;
        //}
        //private static string ToInt(string x)
        //{
        //    int len = x.Length;
        //    string ret, temp;
        //    if (len <= 4)
        //        ret = ChangeInt(x);
        //    else if (len <= 8)
        //    {
        //        ret = ChangeInt(x.Substring(0, len - 4)) + "万";
        //        temp = ChangeInt(x.Substring(len - 4, 4));
        //        if (temp.IndexOf("仟") == -1 && temp != "")
        //            ret += "零" + temp;
        //        else
        //            ret += temp;
        //    }
        //    else
        //    {
        //        ret = ChangeInt(x.Substring(0, len - 8)) + "亿";
        //        temp = ChangeInt(x.Substring(len - 8, 4));
        //        if (temp.IndexOf("仟") == -1 && temp != "")
        //            ret += "零" + temp;
        //        else
        //            ret += temp;
        //        ret += "万";
        //        temp = ChangeInt(x.Substring(len - 4, 4));
        //        if (temp.IndexOf("仟") == -1 && temp != "")
        //            ret += "零" + temp;
        //        else
        //            ret += temp;
        //    }
        //    int i;
        //    if ((i = ret.IndexOf("零万")) != -1)
        //        ret = ret.Remove(i + 1, 1);
        //    while ((i = ret.IndexOf("零零")) != -1)
        //        ret = ret.Remove(i, 1);
        //    if (ret[ret.Length - 1] == '零' && ret.Length > 1)
        //        ret = ret.Remove(ret.Length - 1, 1);
        //    return ret;
        //}
        //private static string ToDot(string x)
        //{
        //    string ret = "";
        //    string sdot = "角分";
        //    if (x.Length > 2) x = x.Substring(0, 2);
        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        ret += ToNum(x[i]) + sdot.Substring(i, 1);
        //    }
        //    if (ret.IndexOf("零角") > -1) ret = ret.Substring(2, 2);
        //    if (ret.IndexOf("零分") > -1) ret = ret.Substring(0, 2);
        //    return ret;
        //}
        //#endregion

        #endregion

        #region 产生62位内任意数字大小写字母的随机数
        /// <summary>
        /// 产生62位内任意数字大小写字母的随机数 
        /// </summary>
        /// <param name="intLen">表示需要产生随机数的数目</param>
        /// <returns></returns>
        public static string GenerateRandom(int intLen)
        {
            char[] constant ={ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < intLen; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }

        #endregion        

        public static bool IsValidIP(string ip)
        {
            string ipPattern = @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$";

            return Regex.IsMatch(ip, ipPattern);
        }


        #endregion
    }
}