using Newtonsoft.Json;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RD3
{
    /// <summary>
    /// 注册帮助类
    /// </summary>
    public class RegUtils
    {
        public static string fileName = Application.StartupPath + @"\register.json";

        //public static string Key = "xz20230414";//key
        /// <summary>
        /// 生成注册码
        /// 防君子不防小人
        /// </summary>
        /// <param name="probation">注册码试用期</param>
        /// <param name="lifespan">注册码有效期</param>
        /// <returns></returns>
        public static string CreateRegNo(int probation, int lifespan, char keystart = '\u0017')
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            //keystart = (char)((byte)' ' - (byte)'\t');
            string regNo = string.Empty;
            foreach (var item in year.ToString())
            {
                regNo += ((char)(item + keystart)).ToString();
            }
            keystart++;

            foreach (var item in month.ToString("00"))
            {
                regNo += ((char)(item + keystart)).ToString();
            }
            keystart++;
            foreach (var item in day.ToString("00"))
            {
                regNo += ((char)(item + keystart)).ToString();
            }
            keystart++;
            foreach (var item in probation.ToString("000"))
            {
                regNo += ((char)(item + keystart)).ToString();
            }
            keystart++;
            foreach (var item in lifespan.ToString())
            {
                regNo += ((char)(item + keystart)).ToString();
            }
            //AnalysisRegNo(regNo);
            return regNo;
        }

        /// <summary>
        /// 重新注册
        /// </summary>
        /// <param name="RegNo"></param>
        public static string ReReg(string RegNo, char keystart = '\u0017')
        {
            string regStrInfo = "";
            try
            {
                int len = 4;
                int startIndex = 0;
                string year = RegNo.Substring(startIndex, len); startIndex += len;
                string year_A = "";
                foreach (var item in year)
                {
                    year_A += ((char)(item - keystart)).ToString();
                }
                len = 2;
                string month = RegNo.Substring(startIndex, len); startIndex += len; keystart++;
                string month_A = "";
                foreach (var item in month)
                {
                    month_A += ((char)(item - keystart)).ToString();
                }

                string day = RegNo.Substring(startIndex, len); startIndex += len; keystart++;
                string day_A = "";
                foreach (var item in day)
                {
                    day_A += ((char)(item - keystart)).ToString();
                }
                len = 3; //注册码试用期
                string probation = RegNo.Substring(startIndex, len); startIndex += len; keystart++;
                string probation_A = "";
                foreach (var item in probation)
                {
                    probation_A += ((char)(item - keystart)).ToString();
                }
                len = 1;//注册码有效期
                string lifespan = RegNo.Substring(startIndex, len); startIndex += len; keystart++;
                string lifespan_A = "";
                foreach (var item in lifespan)
                {
                    lifespan_A += ((char)(item - keystart)).ToString();
                }

                DateTime createTime = new DateTime(int.Parse(year_A), int.Parse(month_A), int.Parse(day_A));
                DateTime lifespanTime = createTime.AddDays(int.Parse(lifespan_A));
                if (createTime > DateTime.Now || lifespanTime < DateTime.Now)
                {
                    regStrInfo = "注册码已失效，请联系管理员获取有效注册码";
                    return regStrInfo;
                }

                //更新注册信息
                RegistrationInfo regInfo = new RegistrationInfo();
                regInfo.StartTime = createTime;
                regInfo.EndTime = createTime.AddDays(int.Parse(probation_A) + 1);
                regInfo.CurrentTime = DateTime.Now;
                string result = JsonConvert.SerializeObject(regInfo);
                result = AESEncryption.Encrypt(result); //加密
                File.WriteAllText(fileName, result);

                regStrInfo = $"注册成功，有效期至:{regInfo.EndTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss")}";
            }
            catch (Exception ex)
            {
                regStrInfo = "非法注册码，请联系管理员获取有效注册码";
            }
            return regStrInfo;
        }

        public static bool IsChecking = true;//检测标识

        /// <summary>
        /// 开始检测
        /// </summary>
        public static void StartChecked()
        {
            new Task(() =>
            {
                while (IsChecking)
                {
                    try
                    {
                        string result = File.ReadAllText(fileName);
                        result = AESEncryption.Decrypt(result); //解密
                        RegistrationInfo regInfo = JsonConvert.DeserializeObject<RegistrationInfo>(result);
                        if (regInfo == null)//如果没有注册文件
                        {
                            MessageBox.Show("注册信息有误，请联系管理员。", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly | MessageBoxOptions.ServiceNotification);
                            Environment.Exit(0);
                        }
                        if (regInfo.CurrentTime > DateTime.Now || regInfo.StartTime > DateTime.Now)//说明改过系统时间，直接退出
                        {
                            MessageBox.Show("注册信息有误，请联系管理员。", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly | MessageBoxOptions.ServiceNotification);
                            Environment.Exit(0);
                        }

                        if (DateTime.Now > regInfo.EndTime)//试用结束，退出
                        {
                            MessageBox.Show("试用时间已到期，请联系管理员。", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly | MessageBoxOptions.ServiceNotification);
                            Environment.Exit(0);
                        }

                        regInfo.CurrentTime = DateTime.Now;
                        result = JsonConvert.SerializeObject(regInfo);
                        result = AESEncryption.Encrypt(result); //加密
                        File.WriteAllText(fileName, result);
                    }
                    catch (Exception ex)
                    {
                        Task.Run(() =>
                        {
                            Thread.Sleep(5 * 60 * 1000);
                            Environment.Exit(0);
                        });
                        MessageBox.Show("注册信息有误，程序将在5分钟之后退出，请联系管理员。","温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly | MessageBoxOptions.ServiceNotification);
                    }
                    finally
                    {
                        Thread.Sleep(60 * 1000);//一分钟检测一次
                    }
                }
            }).Start();
        }

        /// <summary>
        /// 停止检测
        /// </summary>
        public static void StopChecked()
        {
            IsChecking = false;
        }
        /// <summary>
        /// 软件启动，先检测是否有注册
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsReg()
        {
            string result = File.ReadAllText(fileName);
            result = AESEncryption.Decrypt(result); //解密
            RegistrationInfo regInfo = JsonConvert.DeserializeObject<RegistrationInfo>(result);
            if (regInfo == null)//如果没有注册文件
            {
                //MessageBox.Show("注册信息有误，请联系管理员。");
                return false;
            }
            if (regInfo.CurrentTime > DateTime.Now || regInfo.StartTime > DateTime.Now)//说明改过系统时间，直接退出
            {
                //MessageBox.Show("注册信息有误，请联系管理员。");
                return false;
            }

            if (DateTime.Now > regInfo.EndTime)//试用结束，退出
            {
                //MessageBox.Show("试用时间已到期，请联系管理员。");
                return false;
            }
            return true;
        }
    }
}
