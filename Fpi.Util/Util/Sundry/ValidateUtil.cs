using System.Text.RegularExpressions;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// 验证字符串是否合法
    /// </summary>
    public class ValidateUtil
    {
        private static Regex RegNumber = new Regex("^[0-9]+$");
        private static Regex RegNumberSign = new Regex("^[+-]?[0-9]+$");
        private static Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$");
        private static Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$"); //等价于^[+-]?\d+[.]?\d+$

        private static Regex RegEmail =
            new Regex(
                @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

        private static Regex RegPhone =
            new Regex(@"(^[0-9]{3,4}\-[0-9]{3,8}$)|(^[0-9]{3,8}$)|(^\([0-9]{3,4}\)[0-9]{3,8}$)|(^0{0,1}13[0-9]{9}$)");

        private static Regex RegPostCode = new Regex(@"\d{6}");

        public ValidateUtil()
        {
        }

        #region 是否为空字符串

        /// <summary>
        /// 是否为空字符串
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns>true/false</returns>
        public static bool IsBlank(string strInput)
        {
            if (strInput == null || strInput.Trim() == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region 是否数字字符串

        /// <summary>
        /// 是否数字字符串
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumber(string strInput)
        {
            Match m = RegNumber.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否数字字符串 可带正负号

        /// <summary>
        /// 是否数字字符串 可带正负号
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumberSign(string strInput)
        {
            Match m = RegNumberSign.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否是浮点数

        /// <summary>
        /// 是否是浮点数
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimal(string strInput)
        {
            Match m = RegDecimal.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否是浮点数 可带正负号

        /// <summary>
        /// 是否是浮点数 可带正负号
        /// </summary>
        /// <param name="strInput">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimalSign(string strInput)
        {
            Match m = RegDecimalSign.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否为邮件格式

        public static bool IsEmail(string strInput)
        {
            Match m = RegEmail.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否为电话格式

        public static bool IsPhone(string strInput)
        {
            Match m = RegPhone.Match(strInput);
            return m.Success;
        }

        #endregion

        #region 是否为邮编格式

        public static bool IsPostCode(string strInput)
        {
            Match m = RegPostCode.Match(strInput);
            return m.Success;
        }

        #endregion
    }
}