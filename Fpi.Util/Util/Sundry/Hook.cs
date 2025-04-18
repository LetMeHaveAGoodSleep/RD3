//=============================================================================
//使用钩子截获键盘鼠标消息
//创建人:张永强
//创建时间：2011-6-22
//=============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;  
using System.Windows.Forms;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// 钩子基础类
    /// 创建人：张永强
    /// 创建日期：2011-6-22
    /// </summary>
    public class Hook
    {
        private HookTypes hookType;
        public enum HookTypes : int
        {
            WH_KEYBOARD = 2,
            WH_MOUSE = 7,
        }
        //钩子句柄
        protected int hHook = 0;
        protected HookProcHandler hookProcedure; //声明钩子事件类型.  


        //装置钩子的函数  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int SetWindowsHookEx(int idHook, HookProcHandler lpfn, IntPtr hInstance, int threadId);

        //卸下钩子的函数  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern bool UnhookWindowsHookEx(int idHook);

        //下一个钩挂的函数  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        protected static extern int GetCurrentThreadId();

        protected delegate int HookProcHandler(int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>  
        /// 墨认的构造函数构造当前类的实例.  
        /// </summary>  
        public Hook(HookTypes hooktype)
        {
            this.hookType = hooktype;
        }

        //析构函数.  
        ~Hook()
        {
            Stop();
        }

        public void Start()
        {
            //安装鼠标钩子  
            if (hHook == 0)
            {
                //生成一个HookProc的实例.  
                hookProcedure = new HookProcHandler(HookProc);

                hHook = SetWindowsHookEx((int)hookType, hookProcedure, IntPtr.Zero, GetCurrentThreadId());

                //如果装置失败停止钩子  
                if (hHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx failed.");
                }
            }
        }

        public void Stop()
        {
            bool ret = true;
            if (hHook != 0)
            {
                ret = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }

            //如果卸下钩子失败  
            if (!(ret))
                throw new Exception("UnhookWindowsHookEx failed.");
        }

        protected virtual int HookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            return 0;
        }
    }  

    /// <summary>
    /// 鼠标钩子
    /// </summary>
    public class MouseHook :Hook 
    {  
        private const int WM_MOUSEMOVE = 0x200;  
        private const int WM_LBUTTONDOWN = 0x201;  
        private const int WM_RBUTTONDOWN = 0x204;  
        private const int WM_MBUTTONDOWN = 0x207;  
        private const int WM_LBUTTONUP = 0x202;  
        private const int WM_RBUTTONUP = 0x205;  
        private const int WM_MBUTTONUP = 0x208;  
        private const int WM_LBUTTONDBLCLK = 0x203;  
        private const int WM_RBUTTONDBLCLK = 0x206;  
        private const int WM_MBUTTONDBLCLK = 0x209;  
  
        //全局的事件  
        public event MouseEventHandler OnMouseActivity;
        
  
        //声明一个Point的封送类型  
        [StructLayout(LayoutKind.Sequential)]  
        public class POINT   
        {  
            public int x;  
            public int y;  
        }  
  
        //声明鼠标钩子的封送结构类型  
        [StructLayout(LayoutKind.Sequential)]  
        public class MouseHookStruct   
        {  
            public POINT pt;  
            public int hWnd;  
            public int wHitTestCode;  
            public int dwExtraInfo;  
        }  

  
        /// <summary>  
        /// 墨认的构造函数构造当前类的实例.  
        /// </summary>  
        public MouseHook():base(HookTypes.WH_MOUSE)
        {  
        }


        protected override int HookProc(int nCode, int wParam, IntPtr lParam)
        {
            //如果正常运行并且用户要监听鼠标的消息  
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;

                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                }

                //从回调函数中得到鼠标的信息  
                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                MouseEventArgs e = new MouseEventArgs(button, clickCount, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0);
                OnMouseActivity(this, e);
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);   
        }

    }

    /// <summary>
    /// 键盘钩子
    /// </summary>
    public class KeyHook : Hook
    {
        //全局的事件  
        public event KeyEventHandler OnKeyDownActivity;

        /// <summary>  
        /// 墨认的构造函数构造当前类的实例.  
        /// </summary>  
        public KeyHook()
            : base(HookTypes.WH_KEYBOARD)
        {
        }

        protected override int HookProc(int nCode, int wParam, IntPtr lParam)
        {
            //如果正常运行并且用户要监听键盘的消息  
            if ((nCode >= 0) && (OnKeyDownActivity != null))
            {
                Keys keyData = (Keys)wParam;
                if (lParam.ToInt32() > 0)
                {
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    OnKeyDownActivity(this, e);
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

    }

}
