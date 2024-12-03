using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Fpi.Util.WinApiUtil
{
    //事件标志
    public enum EventFlags
    {
        EVENT_PULSE = 1,
        EVENT_RESET = 2,
        EVENT_SET = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }

    public enum APIConstants : uint
    {
        WAIT_OBJECT_0 = 0x00000000,
        WAIT_ABANDONED = 0x00000080,
        WAIT_ABANDONED_0 = 0x00000080,
        WAIT_TIMEOUT = 0x00000102,
        WAIT_FAILED = 0xffffffff,
        INFINITE = 0xffffffff
    }

    public enum ExitWindows : uint
    {
        LOGOFF = 0,
        POWEROFF = 0x00000008,
        REBOOT = 0x00000002,
        SHUTDOWN = 0x00000001,
    }

    public enum ShutdownReason : uint
    {
        SHTDN_REASON_MAJOR_APPLICATION = 0x00040000, // Application issue. 
        SHTDN_REASON_MAJOR_HARDWARE = 0x00010000, // Hardware issue. 
        SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000,
        // The InitiateSystemShutdown function was used instead of InitiateSystemShutdownEx. 
        SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000, // Operating system issue. 
        SHTDN_REASON_MAJOR_OTHER = 0x00000000, // Other issue. 
        SHTDN_REASON_MAJOR_POWER = 0x00060000, // Power failure. 
        SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000, // Software issue. 
        SHTDN_REASON_MAJOR_SYSTEM = 0x00050000, // System failure. 


        SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F, // Blue screen crash event. 
        SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b, // Unplugged. 
        SHTDN_REASON_MINOR_DISK = 0x00000007, // Disk. 
        SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c, // Environment. 
        SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d, // Driver. 
        SHTDN_REASON_MINOR_HOTFIX = 0x00000011, // Hot fix. 
        SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017, // Hot fix uninstallation. 
        SHTDN_REASON_MINOR_HUNG = 0x00000005, // Unresponsive. 
        SHTDN_REASON_MINOR_INSTALLATION = 0x00000002, // Installation. 
        SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001, // Maintenance. 
        SHTDN_REASON_MINOR_MMC = 0x00000019, // MMC issue. 
        SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014, // Network connectivity. 
        SHTDN_REASON_MINOR_NETWORKCARD = 0x00000009, // Network card. 
        SHTDN_REASON_MINOR_OTHER = 0x00000000, // Other issue. 
        SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e, // Other driver event. 
        SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a, // Power supply. 
        SHTDN_REASON_MINOR_PROCESSOR = 0x00000008, // Processor. 
        SHTDN_REASON_MINOR_RECONFIG = 0x00000004, // Reconfigure. 
        SHTDN_REASON_MINOR_SECURITY = 0x00000013, // Security issue. 
        SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012, // Security patch. 
        SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018, // Security patch uninstallation. 
        SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010, // Service pack. 
        SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016, // Service pack uninstallation. 
        SHTND_REASON_MINOR_TERMSRV = 0x00000020, // Terminal Services. 
        SHTDN_REASON_MINOR_UNSTABLE = 0x00000006, // Unstable. 
        SHTDN_REASON_MINOR_UPGRADE = 0x00000003, // Upgrade. 
        SHTDN_REASON_MINOR_WMI = 0x00000015, // WMI issue. 

        SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000,

        /*The reason code is defined by the user. For more information, see Defining a Custom Reason Code. 
If this flag is not present, the reason code is defined by the system.*/

        SHTDN_REASON_FLAG_PLANNED = 0x80000000,
        /*The shutdown was planned. The system generates a System State Data (SSD) file. This file contains system state information such as the processes, threads, memory usage, and configuration. 
If this flag is not present, the shutdown was unplanned. Notification and reporting options are controlled by a set of policies. For example, after logging in, the system displays a dialog box reporting the unplanned shutdown if the policy has been enabled. An SSD file is created only if the SSD policy is enabled on the system. The administrator can use Windows Error Reporting to send the SSD data to a central location, or to Microsoft.*/
    }


}
