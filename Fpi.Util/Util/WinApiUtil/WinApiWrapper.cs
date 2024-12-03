//==========================================================================================
//
// namespace OpenNETCF.IO.Serial.WinApiWrapper
// Copyright (c) 2003, OpenNETCF.org
//
// This library is free software; you can redistribute it and/or modify it under 
// the terms of the OpenNETCF.org Shared Source License.
//
// This library is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the OpenNETCF.org Shared Source License 
// for more details.
//
// You should have received a copy of the OpenNETCF.org Shared Source License 
// along with this library; if not, email licensing@opennetcf.org to request a copy.
//
// If you wish to contact the OpenNETCF Advisory Board to discuss licensing, please 
// email licensing@opennetcf.org.
//
// For general enquiries, email enquiries@opennetcf.org or visit our website at:
// http://www.opennetcf.org
//
//==========================================================================================

using System;
using System.Runtime.InteropServices;
using Fpi.Util.WinApiUtil.CommDataType;

namespace Fpi.Util.WinApiUtil
{
    /// <summary>
    /// 通用WinApi函数包装类
    /// </summary>
    ///
    public class WinApiWrapper
    {
        static WinApiWrapper()
        {
        }

#if WINCE
		private const string DLL_NAME = "coredll.dll";
#else
        private const string DLL_NAME = "kernel32.dll";
#endif

        #region API常量值

        public const Int32 INVALID_HANDLE_VALUE = -1;
        private const UInt32 OPEN_EXISTING = 3;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        private const UInt32 CreateAccess = GENERIC_WRITE | GENERIC_READ;

        #endregion 

        /// <summary>
        /// .NetFramework 平台版本
        /// </summary>
        public static bool FullFramework
        {
            get { return Environment.OSVersion.Platform != PlatformID.WinCE; }
        }

        public static bool WriteFile(IntPtr hPort, byte[] buffer, IntPtr lpOverlapped)
        {
            int cbWritten = 0;
            int totalWritten = 0;
            byte[] tempBuffer = buffer;
            while (true)
            {
                if (!WriteFile(hPort, tempBuffer, tempBuffer.Length, ref cbWritten, lpOverlapped))
                {
                    return false;
                }
                totalWritten += cbWritten;
                if (totalWritten >= buffer.Length)
                    return true;
                //如果没有完全写完，继续写
                tempBuffer = new byte[buffer.Length - totalWritten];
                Buffer.BlockCopy(tempBuffer, 0, buffer, totalWritten, tempBuffer.Length);
            }
        }

        public static bool WriteFile(IntPtr hPort, byte[] buffer)
        {
            return WriteFile(hPort, buffer, IntPtr.Zero);
        }

#if WINCE
		
		#region Windows CE API

		public static bool SetSystemTime(SystemTime st)
		{
			return CESetSystemTime(st);
		}

		public static bool ReadRTCFile(IntPtr hPort, [In,Out] SystemTime st, int cbToRead, ref Int32 cbRead, IntPtr lpOverlapped)
		{
			return Convert.ToBoolean(CEReadRTCFile(hPort,st, cbToRead, ref cbRead, lpOverlapped));
		}

		public static bool WriteRTCFile(IntPtr hPort, SystemTime st, Int32 cbToWrite, ref Int32 cbWritten, IntPtr lpOverlapped)
		{
			return Convert.ToBoolean(CEWriteRTCFile (hPort, st, cbToWrite, ref cbWritten, lpOverlapped));
		}

		public static int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds)
		{
			return CEWaitForSingleObject(hHandle, dwMilliseconds);
		}

		public static IntPtr CreateEvent(bool bManualReset, bool bInitialState, string lpName)
		{
			return CECreateEvent(IntPtr.Zero, Convert.ToInt32(bManualReset), Convert.ToInt32(bInitialState), lpName);
		}
 
		public static bool SetEvent(IntPtr hEvent)
		{
			return Convert.ToBoolean(CEEventModify(hEvent, (uint)EventFlags.EVENT_SET));
		}
 
		public static bool ResetEvent(IntPtr hEvent)
		{
			return Convert.ToBoolean(CEEventModify(hEvent, (uint)EventFlags.EVENT_RESET));
		}
 
		public static bool PulseEvent(IntPtr hEvent)
		{
			return Convert.ToBoolean(CEEventModify(hEvent, (uint)EventFlags.EVENT_PULSE));
		}

		public static IntPtr CreateFile(string FileName)
		{ 
			return CECreateFileW(FileName, CreateAccess, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
		}
 
		public static bool WaitCommEvent(IntPtr hPort, ref CommEventFlags flags) 
		{
			return Convert.ToBoolean(CEWaitCommEvent(hPort, ref flags, IntPtr.Zero));
		}
 
		public static bool ClearCommError(IntPtr hPort, ref CommErrorFlags flags, CommStat stat) 
		{
			return Convert.ToBoolean(CEClearCommError(hPort, ref flags, stat));
		}
 
		public static bool GetCommModemStatus(IntPtr hPort, ref uint lpModemStat)
		{ 
			return Convert.ToBoolean(CEGetCommModemStatus(hPort, ref lpModemStat));
		}
 
		public static bool SetCommMask(IntPtr hPort, CommEventFlags dwEvtMask) 
		{
			return Convert.ToBoolean(CESetCommMask(hPort, dwEvtMask));
		} 
 
		public static bool ReadFile(IntPtr hPort, byte[] buffer, int cbToRead, ref Int32 cbRead, IntPtr lpOverlapped) 
		{
			return Convert.ToBoolean(CEReadFile(hPort, buffer, cbToRead, ref cbRead, IntPtr.Zero));
		} 
		
		public static bool WriteFile(IntPtr hPort, byte[] buffer, Int32 cbToWrite, ref Int32 cbWritten, IntPtr lpOverlapped) 
		{
			return Convert.ToBoolean(CEWriteFile(hPort, buffer, cbToWrite, ref cbWritten, IntPtr.Zero));
		}
 
		public static bool CloseHandle(IntPtr hPort) 
		{
			return Convert.ToBoolean(CECloseHandle(hPort));
		}
 
		public static bool SetupComm(IntPtr hPort, Int32 dwInQueue, Int32 dwOutQueue)
		{
			return Convert.ToBoolean(CESetupComm(hPort, dwInQueue, dwOutQueue));
		}
 
		public static bool SetCommState(IntPtr hPort, DCB dcb) 
		{
			return Convert.ToBoolean(CESetCommState(hPort, dcb));
		}
 
		public static bool GetCommState(IntPtr hPort, DCB dcb) 
		{
			return Convert.ToBoolean(CEGetCommState(hPort, dcb));
		}
 
		public static bool SetCommTimeouts(IntPtr hPort, CommTimeouts timeouts) 
		{ 
			return Convert.ToBoolean(CESetCommTimeouts(hPort, timeouts));
		}
         
		public static bool EscapeCommFunction(IntPtr hPort, CommEscapes escape)
		{
			return Convert.ToBoolean(CEEscapeCommFunction(hPort, (uint)escape));
		}
 
		public static bool DeviceIoControl(IntPtr hPort, Int32 dwIoControlCode, byte[] lpInBuffer, Int32 nInBufferSize, byte[] lpOutBuffer, Int32 nOutBufferSize, ref Int32 lpBytesReturned, IntPtr lpOverlapped ) 
		{
			return Convert.ToBoolean(CEDeviceIoControl(hPort, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, ref lpBytesReturned, lpOverlapped));
		}
		#endregion

		#region Windows CE API imports
 		[DllImport(DLL_NAME, EntryPoint="SetSystemTime", SetLastError = true)]
		private static extern bool CESetSystemTime([In,Out] SystemTime st);

		[DllImport(DLL_NAME, EntryPoint="WaitForSingleObject", SetLastError = true)]
		private static extern int CEWaitForSingleObject(IntPtr hHandle, uint dwMilliseconds); 
 
		[DllImport(DLL_NAME, EntryPoint="EventModify", SetLastError = true)]
		private static extern int CEEventModify(IntPtr hEvent, uint function); 
 
		[DllImport(DLL_NAME, EntryPoint="CreateEvent", SetLastError = true)]
		private static extern IntPtr CECreateEvent(IntPtr lpEventAttributes, int bManualReset, int bInitialState, string lpName); 
 
		[DllImport(DLL_NAME, EntryPoint="EscapeCommFunction", SetLastError = true)]
		private static extern int CEEscapeCommFunction(IntPtr hFile, UInt32 dwFunc);
 
		[DllImport(DLL_NAME, EntryPoint="SetCommTimeouts", SetLastError = true)]
		private static extern int CESetCommTimeouts(IntPtr hFile, CommTimeouts timeouts);
 
		[DllImport(DLL_NAME, EntryPoint="GetCommState", SetLastError = true)]
		private static extern int CEGetCommState(IntPtr hFile, DCB dcb);
 
		[DllImport(DLL_NAME, EntryPoint="SetCommState", SetLastError = true)]
		private static extern int CESetCommState(IntPtr hFile, DCB dcb);
 
		[DllImport(DLL_NAME, EntryPoint="SetupComm", SetLastError = true)]
		private static extern int CESetupComm(IntPtr hFile, Int32 dwInQueue, Int32 dwOutQueue);
 
		[DllImport(DLL_NAME, EntryPoint="CloseHandle", SetLastError = true)]
		private static extern int CECloseHandle(IntPtr hObject);
 
		[DllImport(DLL_NAME, EntryPoint="WriteFile", SetLastError = true)]
		private static extern int CEWriteFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);
		[DllImport(DLL_NAME, EntryPoint="WriteFile", SetLastError = true)]
		private static extern int CEWriteRTCFile(IntPtr hFile, [In,Out] SystemTime st, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);
 
		[DllImport(DLL_NAME, EntryPoint="ReadFile", SetLastError = true)]
		private static extern int CEReadFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);
 
		[DllImport(DLL_NAME, EntryPoint="ReadFile", SetLastError = true)]
		private static extern int CEReadRTCFile(IntPtr hFile, [In,Out] SystemTime st, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);
 
		[DllImport(DLL_NAME, EntryPoint="SetCommMask", SetLastError = true)]
		private static extern int CESetCommMask(IntPtr handle, CommEventFlags dwEvtMask);
 
		[DllImport(DLL_NAME, EntryPoint="GetCommModemStatus", SetLastError = true)]
		private static extern int CEGetCommModemStatus(IntPtr hFile, ref uint lpModemStat);
 
		[DllImport(DLL_NAME, EntryPoint="ClearCommError", SetLastError = true)]
		private static extern int CEClearCommError(IntPtr hFile, ref CommErrorFlags lpErrors, CommStat lpStat);
 
		[DllImport(DLL_NAME, EntryPoint="WaitCommEvent", SetLastError = true)]
		private static extern int CEWaitCommEvent(IntPtr hFile, ref CommEventFlags lpEvtMask, IntPtr lpOverlapped);
         
		[DllImport(DLL_NAME, EntryPoint="CreateFileW", SetLastError = true)]
		private static extern IntPtr CECreateFileW(
			String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport(DLL_NAME, EntryPoint="DeviceIoControl", SetLastError = true)]
		private static extern int CEDeviceIoControl(IntPtr hFile, Int32 dwIoControlCode, byte[] lpInBuffer, Int32 nInBufferSize, byte[] lpOutBuffer, Int32 nOutBufferSize, ref Int32 lpBytesReturned, IntPtr lpOverlapped);
		#endregion

#else

        #region Desktop Windows API

        public static IntPtr LocalFree(IntPtr hMem)
        {
            return WinLocalFree(hMem);
        }

        public static IntPtr LocalAlloc(int uFlags, int uBytes)
        {
            return WinLocalAlloc(uFlags, uBytes);
        }

        public static int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds)
        {
            return WinWaitForSingleObject(hHandle, dwMilliseconds);
        }

        public static IntPtr CreateEvent(bool bManualReset, bool bInitialState, string lpName)
        {
            return WinCreateEvent(IntPtr.Zero, Convert.ToInt32(bManualReset), Convert.ToInt32(bInitialState), lpName);
        }

        public static bool SetEvent(IntPtr hEvent)
        {
            return Convert.ToBoolean(WinSetEvent(hEvent));
        }

        public static bool ResetEvent(IntPtr hEvent)
        {
            return Convert.ToBoolean(WinResetEvent(hEvent));
        }

        public static bool PulseEvent(IntPtr hEvent)
        {
            return Convert.ToBoolean(WinPulseEvent(hEvent));
        }

        public static IntPtr CreateFile(string FileName)
        {
            return
                WinCreateFileW(FileName, CreateAccess, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
        }

        public static bool WaitCommEvent(IntPtr hPort, ref CommEventFlags flags)
        {
            return Convert.ToBoolean(WinWaitCommEvent(hPort, ref flags, IntPtr.Zero));
        }

        public static bool ClearCommError(IntPtr hPort, ref CommErrorFlags flags, CommStat stat)
        {
            return Convert.ToBoolean(WinClearCommError(hPort, ref flags, stat));
        }

        public static bool GetCommModemStatus(IntPtr hPort, ref uint lpModemStat)
        {
            return Convert.ToBoolean(WinGetCommModemStatus(hPort, ref lpModemStat));
        }

        public static bool SetCommMask(IntPtr hPort, CommEventFlags dwEvtMask)
        {
            return Convert.ToBoolean(WinSetCommMask(hPort, dwEvtMask));
        }

        public static bool ReadFile(IntPtr hPort, byte[] buffer, int cbToRead, ref Int32 cbRead, IntPtr lpOverlapped)
        {
            return Convert.ToBoolean(WinReadFile(hPort, buffer, cbToRead, ref cbRead, lpOverlapped));
        }

        public static bool WriteFile(IntPtr hPort, byte[] buffer, Int32 cbToWrite, ref Int32 cbWritten,
                                     IntPtr lpOverlapped)
        {
            return Convert.ToBoolean(WinWriteFile(hPort, buffer, cbToWrite, ref cbWritten, lpOverlapped));
        }

        public static bool DeviceIoControl(IntPtr hPort, Int32 dwIoControlCode, byte[] lpInBuffer, Int32 nInBufferSize,
                                           byte[] lpOutBuffer, Int32 nOutBufferSize, ref Int32 lpBytesReturned,
                                           IntPtr lpOverlapped)
        {
            return
                Convert.ToBoolean(
                    WinDeviceIoControl(hPort, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize,
                                       ref lpBytesReturned, lpOverlapped));
        }

        public static bool CloseHandle(IntPtr hPort)
        {
            return Convert.ToBoolean(WinCloseHandle(hPort));
        }

        public static bool SetupComm(IntPtr hPort, Int32 dwInQueue, Int32 dwOutQueue)
        {
            return Convert.ToBoolean(WinSetupComm(hPort, dwInQueue, dwOutQueue));
        }

        public static bool SetCommState(IntPtr hPort, DCB dcb)
        {
            return Convert.ToBoolean(WinSetCommState(hPort, dcb));
        }

        public static bool GetCommState(IntPtr hPort, DCB dcb)
        {
            return Convert.ToBoolean(WinGetCommState(hPort, dcb));
        }

        public static bool SetCommTimeouts(IntPtr hPort, CommTimeouts timeouts)
        {
            return Convert.ToBoolean(WinSetCommTimeouts(hPort, timeouts));
        }

        public static bool EscapeCommFunction(IntPtr hPort, CommEscapes escape)
        {
            return Convert.ToBoolean(WinEscapeCommFunction(hPort, (uint) escape));
        }

        #endregion

        #region Desktop Windows API imports

        [DllImport(DLL_NAME, EntryPoint="LocalAlloc", SetLastError=true)]
        private static extern IntPtr WinLocalAlloc(int uFlags, int uBytes);

        [DllImport(DLL_NAME, EntryPoint="LocalFree", SetLastError=true)]
        private static extern IntPtr WinLocalFree(IntPtr hMem);

        [DllImport(DLL_NAME, EntryPoint="WaitForSingleObject", SetLastError = true)]
        private static extern int WinWaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport(DLL_NAME, EntryPoint="SetEvent", SetLastError = true)]
        private static extern int WinSetEvent(IntPtr hEvent);

        [DllImport(DLL_NAME, EntryPoint="ResetEvent", SetLastError = true)]
        private static extern int WinResetEvent(IntPtr hEvent);

        [DllImport(DLL_NAME, EntryPoint="PulseEvent", SetLastError = true)]
        private static extern int WinPulseEvent(IntPtr hEvent);

        [DllImport(DLL_NAME, EntryPoint="CreateEvent", SetLastError = true)]
        private static extern IntPtr WinCreateEvent(IntPtr lpEventAttributes, int bManualReset, int bInitialState,
                                                    string lpName);

        [DllImport(DLL_NAME, EntryPoint="EscapeCommFunction", SetLastError = true)]
        private static extern int WinEscapeCommFunction(IntPtr hFile, UInt32 dwFunc);

        [DllImport(DLL_NAME, EntryPoint="SetCommTimeouts", SetLastError = true)]
        private static extern int WinSetCommTimeouts(IntPtr hFile, CommTimeouts timeouts);

        [DllImport(DLL_NAME, EntryPoint="GetCommState", SetLastError = true)]
        private static extern int WinGetCommState(IntPtr hFile, DCB dcb);

        [DllImport(DLL_NAME, EntryPoint="SetCommState", SetLastError = true)]
        private static extern int WinSetCommState(IntPtr hFile, DCB dcb);

        [DllImport(DLL_NAME, EntryPoint="SetupComm", SetLastError = true)]
        private static extern int WinSetupComm(IntPtr hFile, Int32 dwInQueue, Int32 dwOutQueue);

        [DllImport(DLL_NAME, EntryPoint="CloseHandle", SetLastError = true)]
        private static extern int WinCloseHandle(IntPtr hObject);

        [DllImport(DLL_NAME, EntryPoint="WriteFile", SetLastError = true)]
        private static extern int WinWriteFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToRead,
                                               ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport(DLL_NAME, EntryPoint="ReadFile", SetLastError = true)]
        private static extern int WinReadFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToRead,
                                              ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport(DLL_NAME, EntryPoint="SetCommMask", SetLastError = true)]
        private static extern int WinSetCommMask(IntPtr handle, CommEventFlags dwEvtMask);

        [DllImport(DLL_NAME, EntryPoint="GetCommModemStatus", SetLastError = true)]
        private static extern int WinGetCommModemStatus(IntPtr hFile, ref uint lpModemStat);

        [DllImport(DLL_NAME, EntryPoint="ClearCommError", SetLastError = true)]
        private static extern int WinClearCommError(IntPtr hFile, ref CommErrorFlags lpErrors, CommStat lpStat);

        [DllImport(DLL_NAME, EntryPoint="CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr WinCreateFileW(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
                                                    IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition,
                                                    UInt32 dwFlagsAndAttributes,
                                                    IntPtr hTemplateFile);

        [DllImport(DLL_NAME, EntryPoint="WaitCommEvent", SetLastError = true)]
        private static extern int WinWaitCommEvent(IntPtr hFile, ref CommEventFlags lpEvtMask, IntPtr lpOverlapped);

        [DllImport(DLL_NAME, EntryPoint="DeviceIoControl", SetLastError = true)]
        private static extern int WinDeviceIoControl(IntPtr hFile, Int32 dwIoControlCode, byte[] lpInBuffer,
                                                     Int32 nInBufferSize, byte[] lpOutBuffer, Int32 nOutBufferSize,
                                                     ref Int32 lpBytesReturned, IntPtr lpOverlapped);
        
        [DllImport(DLL_NAME, EntryPoint = "LoadLibrary", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport(DLL_NAME, EntryPoint = "FreeLibrary", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr lib);

        #endregion
#endif
    }
}