#if !WINCE
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Fpi.Util.WinApiUtil
{
    public enum RestartOptions
    {
        LogOff = 0,
        PowerOff = 8,
        Reboot = 2,
        ShutDown = 1,
        Suspend = -1,
        Hibernate = -2,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LUID
    {
        public int LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LUID_AND_ATTRIBUTES
    {
        public LUID pLuid;
        public int Attributes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges;
    }

    public class WindowsController
    {
        private const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        private const int TOKEN_QUERY = 0x8;
        private const int SE_PRIVILEGE_ENABLED = 0x2;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int EWX_FORCE = 4;

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", CharSet = CharSet.Ansi)]
        private static extern int FreeLibrary(IntPtr hLibModule);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("powrprof.dll", EntryPoint = "SetSuspendState", CharSet = CharSet.Ansi)]
        private static extern int SetSuspendState(int Hibernate, int ForceCritical, int DisableWakeEvent);

        [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", CharSet = CharSet.Ansi)]
        private static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueA", CharSet = CharSet.Ansi)]
        private static extern int LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", CharSet = CharSet.Ansi)]
        private static extern int AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPrivileges,
                                                        ref TOKEN_PRIVILEGES NewState, int BufferLength,
                                                        ref TOKEN_PRIVILEGES PreviousState, ref int ReturnLength);

        [DllImport("user32.dll", EntryPoint = "ExitWindowsEx", CharSet = CharSet.Ansi)]
        private static extern int ExitWindowsEx(int uFlags, int dwReserved);

        [DllImport("user32.dll", EntryPoint = "FormatMessageA", CharSet = CharSet.Ansi)]
        private static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId,
                                                StringBuilder lpBuffer, int nSize, int Arguments);

        public static void ExitWindows(RestartOptions how, bool force)
        {
            switch (how)
            {
                case RestartOptions.Suspend:
                    {
                        SuspendSystem(false, force);
                        break;
                    }
                case RestartOptions.Hibernate:
                    {
                        SuspendSystem(true, force);
                        break;
                    }
                default:
                    {
                        ExitWindows((int) how, force);
                        break;
                    }
            }
        }

        protected static void ExitWindows(int how, bool force)
        {
            EnableToken("SeShutdownPrivilege");
            if (force) how = how | EWX_FORCE;
            if (ExitWindowsEx(how, 0) == 0)
                throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
        }

        protected static void EnableToken(string privilege)
        {
            if (!CheckEntryPoint("advapi32.dll", "AdjustTokenPrivileges"))
                return;
            IntPtr tokenHandle = IntPtr.Zero;
            LUID privilegeLUID = new LUID();
            TOKEN_PRIVILEGES newPrivileges = new TOKEN_PRIVILEGES();
            TOKEN_PRIVILEGES tokenPrivileges;
            if (
                OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                                 ref tokenHandle) == 0)
                throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
            if (LookupPrivilegeValue("", privilege, ref privilegeLUID) == 0)
                throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
            tokenPrivileges.Privileges.pLuid = privilegeLUID;
            int size = 4;
            if (
                AdjustTokenPrivileges(tokenHandle, 0, ref tokenPrivileges, 4 + (12*tokenPrivileges.PrivilegeCount),
                                      ref newPrivileges, ref size) == 0)
                throw new PrivilegeException(FormatError(Marshal.GetLastWin32Error()));
        }

        protected static void SuspendSystem(bool hibernate, bool force)
        {
            if (!CheckEntryPoint("powrprof.dll", "SetSuspendState"))
                throw new PlatformNotSupportedException(
                    "The   SetSuspendState   method   is   not   supported   on   this   system!");
            SetSuspendState((int) (hibernate ? 1 : 0), (int) (force ? 1 : 0), 0);
        }

        protected static bool CheckEntryPoint(string library, string method)
        {
            IntPtr libPtr = LoadLibrary(library);
            if (!libPtr.Equals(IntPtr.Zero))
            {
                if (!GetProcAddress(libPtr, method).Equals(IntPtr.Zero))
                {
                    FreeLibrary(libPtr);
                    return true;
                }
                FreeLibrary(libPtr);
            }
            FreeLibrary(libPtr);
            return false;
        }

        protected static string FormatError(int number)
        {
            StringBuilder buffer = new StringBuilder(255);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, number, 0, buffer, buffer.Capacity, 0);
            return buffer.ToString();
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, // Ŀ�� DC�ľ��
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc, // ԴDC�ľ��
            int nXSrc,
            int nYSrc,
            Int32 dwRop // ��դ�Ĵ�����ֵ
            );

        public static Image PrintScreen(Control ctr)
        {
            if (ctr.InvokeRequired)
            {
                return (Image) ctr.Invoke(new CaptureScreenHandler(CaptureScreenProc), new object[] {ctr});
            }
            else
            {
                return CaptureScreenProc(ctr);
            }
        }

        private delegate Image CaptureScreenHandler(Control ctr);

        public static Image CaptureScreenProc(Control ctr)
        {
            //��õ�ǰ��Ļ�Ĵ�С
            Rectangle rect = new Rectangle();
            rect = Screen.GetWorkingArea(ctr);

            //����һ���Ե�ǰ��ĻΪģ���ͼ��
            Graphics g1 = ctr.CreateGraphics();

            //��������Ļ��СΪ��׼��λͼ 
            Image MyImage = new Bitmap(rect.Width, rect.Height, g1);
            Graphics g2 = Graphics.FromImage(MyImage);

            //�õ���Ļ��DC
            IntPtr dc1 = g1.GetHdc();

            //�õ�Bitmap��DC 
            IntPtr dc2 = g2.GetHdc();

            //���ô�API������ʵ����Ļ����
            BitBlt(dc2, 0, 0, rect.Width, rect.Height, dc1, 0, 0, 13369376);
            //�ͷŵ���Ļ��DC
            g1.ReleaseHdc(dc1);
            //�ͷŵ�Bitmap��DC 
            g2.ReleaseHdc(dc2);

            g1.Dispose();
            g2.Dispose();

            return MyImage;
        }
    }

    public class PrivilegeException : Exception
    {
        public PrivilegeException() : base()
        {
        }

        public PrivilegeException(string message) : base(message)
        {
        }
    }
}

#endif