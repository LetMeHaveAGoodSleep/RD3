using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
//串口数据结构定义

namespace Fpi.Util.WinApiUtil.CommDataType
{
    [StructLayout(LayoutKind.Sequential)]
    public class CommTimeouts
    {
        public UInt32 ReadIntervalTimeout;
        public UInt32 ReadTotalTimeoutMultiplier;
        public UInt32 ReadTotalTimeoutConstant;
        public UInt32 WriteTotalTimeoutMultiplier;
        public UInt32 WriteTotalTimeoutConstant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OVERLAPPED
    {
        public UIntPtr Internal;
        public UIntPtr InternalHigh;
        public UInt32 Offset;
        public UInt32 OffsetHigh;
        public IntPtr hEvent;
    }

    /// <summary>
    /// Event Flags
    /// </summary>
    [Flags]
    public enum CommEventFlags : int
    {
        /// <summary>
        /// No flags
        /// </summary>
        NONE = 0x0000, //
        /// <summary>
        /// Event on receive
        /// </summary>
        RXCHAR = 0x0001, // Any Character received
        /// <summary>
        /// Event when specific character is received
        /// </summary>
        RXFLAG = 0x0002, // Received specified flag character
        /// <summary>
        /// Event when the transmit buffer is empty
        /// </summary>
        TXEMPTY = 0x0004, // Tx buffer Empty
        /// <summary>
        /// Event on CTS state change
        /// </summary>
        CTS = 0x0008, // CTS changed
        /// <summary>
        /// Event on DSR state change
        /// </summary>
        DSR = 0x0010, // DSR changed
        /// <summary>
        /// Event on RLSD state change
        /// </summary>
        RLSD = 0x0020, // RLSD changed
        /// <summary>
        /// Event on BREAK
        /// </summary>
        BREAK = 0x0040, // BREAK received
        /// <summary>
        /// Event on line error
        /// </summary>
        ERR = 0x0080, // Line status error
        /// <summary>
        /// Event on ring detect
        /// </summary>
        RING = 0x0100, // ring detected
        /// <summary>
        /// Event on printer error
        /// </summary>
        PERR = 0x0200, // printer error
        /// <summary>
        /// Event on 80% high-water
        /// </summary>
        RX80FULL = 0x0400, // rx buffer is at 80%
        /// <summary>
        /// Provider event 1
        /// </summary>
        EVENT1 = 0x0800, // provider event
        /// <summary>
        /// Provider event 2
        /// </summary>
        EVENT2 = 0x1000, // provider event
        /// <summary>
        /// Event on CE power notification
        /// </summary>
        POWER = 0x2000, // wince power notification
        /// <summary>
        /// Mask for all flags under CE
        /// </summary>
        ALLCE = 0x3FFF, // mask of all flags for CE
        /// <summary>
        /// Mask for all flags under desktop Windows
        /// </summary>
        ALLPC = BREAK | CTS | DSR | ERR | RING | RLSD | RXCHAR | RXFLAG | TXEMPTY
    }


    /// <summary>
    /// Error flags
    /// </summary>
    [Flags]
    public enum CommErrorFlags : int
    {
        /// <summary>
        /// Receive overrun
        /// </summary>
        RXOVER = 0x0001,
        /// <summary>
        /// Overrun
        /// </summary>
        OVERRUN = 0x0002,
        /// <summary>
        /// Parity error
        /// </summary>
        RXPARITY = 0x0004,
        /// <summary>
        /// Frame error
        /// </summary>
        FRAME = 0x0008,
        /// <summary>
        /// BREAK received
        /// </summary>
        BREAK = 0x0010,
        /// <summary>
        /// Transmit buffer full
        /// </summary>
        TXFULL = 0x0100,
        /// <summary>
        /// IO Error
        /// </summary>
        IOE = 0x0400,
        /// <summary>
        /// Requested mode not supported
        /// </summary>
        MODE = 0x8000
    }

    /// <summary>
    /// Modem status flags
    /// </summary>
    [Flags]
    public enum CommModemStatusFlags : int
    {
        /// <summary>
        /// The CTS (Clear To Send) signal is on.
        /// </summary>
        MS_CTS_ON = 0x0010,
        /// <summary>
        /// The DSR (Data Set Ready) signal is on.
        /// </summary>
        MS_DSR_ON = 0x0020,
        /// <summary>
        /// The ring indicator signal is on.
        /// </summary>
        MS_RING_ON = 0x0040,
        /// <summary>
        /// The RLSD (Receive Line Signal Detect) signal is on.
        /// </summary>
        MS_RLSD_ON = 0x0080
    }

    /// <summary>
    /// Communication escapes
    /// </summary>
    public enum CommEscapes : uint
    {
        /// <summary>
        /// Causes transmission to act as if an XOFF character has been received.
        /// </summary>
        SETXOFF = 1,
        /// <summary>
        /// Causes transmission to act as if an XON character has been received.
        /// </summary>
        SETXON = 2,
        /// <summary>
        /// Sends the RTS (Request To Send) signal.
        /// </summary>
        SETRTS = 3,
        /// <summary>
        /// Clears the RTS (Request To Send) signal
        /// </summary>
        CLRRTS = 4,
        /// <summary>
        /// Sends the DTR (Data Terminal Ready) signal.
        /// </summary>
        SETDTR = 5,
        /// <summary>
        /// Clears the DTR (Data Terminal Ready) signal.
        /// </summary>
        CLRDTR = 6,
        /// <summary>
        /// Suspends character transmission and places the transmission line in a break state until the ClearCommBreak function is called (or EscapeCommFunction is called with the CLRBREAK extended function code). The SETBREAK extended function code is identical to the SetCommBreak function. This extended function does not flush data that has not been transmitted.
        /// </summary>
        SETBREAK = 8,
        /// <summary>
        /// Restores character transmission and places the transmission line in a nonbreak state. The CLRBREAK extended function code is identical to the ClearCommBreak function
        /// </summary>
        CLRBREAK = 9,
        ///Set the port to IR mode.
        SETIR = 10,
        /// <summary>
        /// Set the port to non-IR mode.
        /// </summary>
        CLRIR = 11
    }

    /// <summary>
    /// Error values from serial API calls
    /// </summary>
    public enum APIErrors : int
    {
        /// <summary>
        /// Port not found
        /// </summary>
        ERROR_FILE_NOT_FOUND = 2,
        /// <summary>
        /// Invalid port name
        /// </summary>
        ERROR_INVALID_NAME = 123,
        /// <summary>
        /// Access denied
        /// </summary>
        ERROR_ACCESS_DENIED = 5,
        /// <summary>
        /// invalid handle
        /// </summary>
        ERROR_INVALID_HANDLE = 6,
        /// <summary>
        /// IO pending
        /// </summary>
        ERROR_IO_PENDING = 997
    }


    [StructLayout(LayoutKind.Sequential)]
    public class CommStat
    {
        //
        // typedef struct _COMSTAT {
        // DWORD fCtsHold : 1;
        // DWORD fDsrHold : 1;
        // DWORD fRlsdHold : 1;
        // DWORD fXoffHold : 1;
        // DWORD fXoffSent : 1;
        // DWORD fEof : 1;
        // DWORD fTxim : 1;
        // DWORD fReserved : 25;
        // DWORD cbInQue;
        // DWORD cbOutQue;
        // } COMSTAT, *LPCOMSTAT;
        //

        private BitVector32 bitfield = new BitVector32(0); // UKI added for CLR bitfield support

        public UInt32 cbInQue = 0;
        public UInt32 cbOutQue = 0;

        // Helper constants for manipulating the bit fields.

        [Flags]
        private enum commFlags
        {
            fCtsHoldMask = 0x01,
            fDsrHoldMask = 0x02,
            fRlsdHoldMask = 0x04,
            fXoffHoldMask = 0x08,
            fXoffSentMask = 0x10,
            fEofMask = 0x20,
            fTximMask = 0x40
        } ;

        public bool fCtsHold
        {
            get { return bitfield[(int) commFlags.fCtsHoldMask]; }
            set { bitfield[(int) commFlags.fCtsHoldMask] = value; }
        }

        public bool fDsrHold
        {
            get { return bitfield[(int) commFlags.fDsrHoldMask]; }
            set { bitfield[(int) commFlags.fDsrHoldMask] = value; }
        }

        public bool fRlsdHold
        {
            get { return bitfield[(int) commFlags.fRlsdHoldMask]; }
            set { bitfield[(int) commFlags.fRlsdHoldMask] = value; }
        }

        public bool fXoffHold
        {
            get { return bitfield[(int) commFlags.fXoffHoldMask]; }
            set { bitfield[(int) commFlags.fXoffHoldMask] = value; }
        }

        public bool fXoffSent
        {
            get { return bitfield[(int) commFlags.fXoffSentMask]; }
            set { bitfield[(int) commFlags.fXoffSentMask] = value; }
        }

        public bool fEof
        {
            get { return bitfield[(int) commFlags.fEofMask]; }
            set { bitfield[(int) commFlags.fEofMask] = value; }
        }

        public bool fTxim
        {
            get { return bitfield[(int) commFlags.fTximMask]; }
            set { bitfield[(int) commFlags.fTximMask] = value; }
        }
    }
}