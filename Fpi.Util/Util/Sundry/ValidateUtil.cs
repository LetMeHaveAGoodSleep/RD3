using System.Text.RegularExpressions;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// ��֤�ַ����Ƿ�Ϸ�
    /// </summary>
    public class ValidateUtil
    {
        private static Regex RegNumber = new Regex("^[0-9]+$");
        private static Regex RegNumberSign = new Regex("^[+-]?[0-9]+$");
        private static Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$");
        private static Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$"); //�ȼ���^[+-]?\d+[.]?\d+$

        private static Regex RegEmail =
            new Regex(
                @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

        private static Regex RegPhone =
            new Regex(@"(^[0-9]{3,4}\-[0-9]{3,8}$)|(^[0-9]{3,8}$)|(^\([0-9]{3,4}\)[0-9]{3,8}$)|(^0{0,1}13[0-9]{9}$)");

        private static Regex RegPostCode = new Regex(@"\d{6}");

        public ValidateUtil()
        {
        }

        #region �Ƿ�Ϊ���ַ���

        /// <summary>
        /// �Ƿ�Ϊ���ַ���
        /// </summary>
        /// <param name="strInput">�����ַ���</param>
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

        #region �Ƿ������ַ���

        /// <summary>
        /// �Ƿ������ַ���
        /// </summary>
        /// <param name="strInput">�����ַ���</param>
        /// <returns></returns>
        public static bool IsNumber(string strInput)
        {
            Match m = RegNumber.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ������ַ��� �ɴ�������

        /// <summary>
        /// �Ƿ������ַ��� �ɴ�������
        /// </summary>
        /// <param name="strInput">�����ַ���</param>
        /// <returns></returns>
        public static bool IsNumberSign(string strInput)
        {
            Match m = RegNumberSign.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ��Ǹ�����

        /// <summary>
        /// �Ƿ��Ǹ�����
        /// </summary>
        /// <param name="strInput">�����ַ���</param>
        /// <returns></returns>
        public static bool IsDecimal(string strInput)
        {
            Match m = RegDecimal.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ��Ǹ����� �ɴ�������

        /// <summary>
        /// �Ƿ��Ǹ����� �ɴ�������
        /// </summary>
        /// <param name="strInput">�����ַ���</param>
        /// <returns></returns>
        public static bool IsDecimalSign(string strInput)
        {
            Match m = RegDecimalSign.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ�Ϊ�ʼ���ʽ

        public static bool IsEmail(string strInput)
        {
            Match m = RegEmail.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ�Ϊ�绰��ʽ

        public static bool IsPhone(string strInput)
        {
            Match m = RegPhone.Match(strInput);
            return m.Success;
        }

        #endregion

        #region �Ƿ�Ϊ�ʱ��ʽ

        public static bool IsPostCode(string strInput)
        {
            Match m = RegPostCode.Match(strInput);
            return m.Success;
        }

        #endregion
    }
}