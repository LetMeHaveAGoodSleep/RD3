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
//    char       m_userid[12];   	       //�ն�ģ�����
//    uint32     m_sin_addr;     	       //�ն�ģ�����Internet�Ĵ�������IP��ַ
//    uint16     m_sin_port;     	       //�ն�ģ�����Internet�Ĵ�������IP�˿�
//    uint32     m_local_addr;   	       //�ն�ģ�����ƶ�����IP��ַ
//    uint16     m_local_port;   	       //�ն�ģ�����ƶ�����IP�˿�
//    char       m_logon_date[20]; 	  //�ն�ģ���¼ʱ��
//    char       m_update_date[20];      //�ն�ģ�����ʱ�䣬ʹ��ǰ���ֽڣ�time_t����
//��16�ֽ�δʹ�ã�
//    uint8	      m_status;		      //�ն�ģ��״̬, 1 ���� 0 ������
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
    /// GprsUserInfo ��ժҪ˵����
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
