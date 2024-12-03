//==========================================================================================
//
// namespace FPIServer
// Copyright (c) 2005, Focused Photonics Inc
//
//This class prase userinfo struct to C# Type
//
// The userinfo struct in DLL:
//typedef struct GPRS_USER_INFO
//{
//    char       m_userid[12];   	       //终端模块号码
//    uint32     m_sin_addr;     	       //终端模块进入Internet的代理主机IP地址
//    uint16     m_sin_port;     	       //终端模块进入Internet的代理主机IP端口
//    uint32     m_local_addr;   	       //终端模块在移动网内IP地址
//    uint16     m_local_port;   	       //终端模块在移动网内IP端口
//    char       m_logon_date[20]; 	  //终端模块登录时间
//    char       m_update_date[20];      //终端模块更新时间，使用前四字节，time_t类型
//后16字节未使用；
//    uint8	      m_status;		      //终端模块状态, 1 在线 0 不在线
//}user_info;
//
//Store in \Communication\Gprs\GprsUserInfo.cs
//Creater/Modifyer:				Date:
//ModifyDesc:
//
//
//==========================================================================================
using System;

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// GprsUserInfo 的摘要说明。
    /// </summary>
    public class GprsUserInfo
    {
        public string userId;
        public string sinAddr;
        public ushort sinPort;

        public string localAddr;
        public ushort localPort;

        public string logonTime;
        public string updateTime;

        public bool status;     // 1 online, 0 not online

        public GprsUserInfo()
        {
        }

        public void Parse(byte[] data)
        {

            // user id
            userId = "";
            for (int i = 0; i < 12; i++)
            {
                userId += (char)data[i];
            }

            // sin addr 
            sinAddr = "";
            sinAddr += data[15] + "." + data[14] + "." + data[13] + "." + data[12];

            // sin port
            sinPort = (ushort)(data[16] | (data[17] << 8));

            localAddr = "";
            localAddr += data[21] + "." + data[20] + "." + data[19] + "." + data[18];

            localPort = (ushort)(data[22] | (data[23] << 8));

            //Logon time
            logonTime = "";
            for (int i = 26; i < 45; i++)
            {
                logonTime += (char)data[i];
            }

            //update Time
            DateTime baseTime = new DateTime(1970, 1, 1);
            double addSeconed = 0;
            for (int i = 46; i < 50; i++)
            {
                addSeconed = addSeconed + ((int)data[i]) * Math.Pow(2, (i - 46) * 8);

            }

            DateTime updateTimeFormat = baseTime.AddSeconds(addSeconed);
            updateTimeFormat = updateTimeFormat.AddHours(8);

            updateTime = updateTimeFormat.ToString("u");

            updateTime = updateTime.Substring(0, updateTime.Length - 1);

        }
    }
}
