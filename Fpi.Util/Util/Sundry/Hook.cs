//=============================================================================
//ʹ�ù��ӽػ���������Ϣ
//������:����ǿ
//����ʱ�䣺2011-6-22
//=============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;  
using System.Windows.Forms;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// ���ӻ�����
    /// �����ˣ�����ǿ
    /// �������ڣ�2011-6-22
    /// </summary>
    public class Hook
    {
        private HookTypes hookType;
        public enum HookTypes : int
        {
            WH_KEYBOARD = 2,
            WH_MOUSE = 7,
        }
        //���Ӿ��
        protected int hHook = 0;
        protected HookProcHandler hookProcedure; //���������¼�����.  


        //װ�ù��ӵĺ���  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int SetWindowsHookEx(int idHook, HookProcHandler lpfn, IntPtr hInstance, int threadId);

        //ж�¹��ӵĺ���  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern bool UnhookWindowsHookEx(int idHook);

        //��һ�����ҵĺ���  
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        protected static extern int GetCurrentThreadId();

        protected delegate int HookProcHandler(int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>  
        /// ī�ϵĹ��캯�����쵱ǰ���ʵ��.  
        /// </summary>  
        public Hook(HookTypes hooktype)
        {
            this.hookType = hooktype;
        }

        //��������.  
        ~Hook()
        {
            Stop();
        }

        public void Start()
        {
            //��װ��깳��  
            if (hHook == 0)
            {
                //����һ��HookProc��ʵ��.  
                hookProcedure = new HookProcHandler(HookProc);

                hHook = SetWindowsHookEx((int)hookType, hookProcedure, IntPtr.Zero, GetCurrentThreadId());

                //���װ��ʧ��ֹͣ����  
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

            //���ж�¹���ʧ��  
            if (!(ret))
                throw new Exception("UnhookWindowsHookEx failed.");
        }

        protected virtual int HookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            return 0;
        }
    }  

    /// <summary>
    /// ��깳��
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
  
        //ȫ�ֵ��¼�  
        public event MouseEventHandler OnMouseActivity;
        
  
        //����һ��Point�ķ�������  
        [StructLayout(LayoutKind.Sequential)]  
        public class POINT   
        {  
            public int x;  
            public int y;  
        }  
  
        //������깳�ӵķ��ͽṹ����  
        [StructLayout(LayoutKind.Sequential)]  
        public class MouseHookStruct   
        {  
            public POINT pt;  
            public int hWnd;  
            public int wHitTestCode;  
            public int dwExtraInfo;  
        }  

  
        /// <summary>  
        /// ī�ϵĹ��캯�����쵱ǰ���ʵ��.  
        /// </summary>  
        public MouseHook():base(HookTypes.WH_MOUSE)
        {  
        }


        protected override int HookProc(int nCode, int wParam, IntPtr lParam)
        {
            //����������в����û�Ҫ����������Ϣ  
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

                //�ӻص������еõ�������Ϣ  
                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                MouseEventArgs e = new MouseEventArgs(button, clickCount, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0);
                OnMouseActivity(this, e);
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);   
        }

    }

    /// <summary>
    /// ���̹���
    /// </summary>
    public class KeyHook : Hook
    {
        //ȫ�ֵ��¼�  
        public event KeyEventHandler OnKeyDownActivity;

        /// <summary>  
        /// ī�ϵĹ��캯�����쵱ǰ���ʵ��.  
        /// </summary>  
        public KeyHook()
            : base(HookTypes.WH_KEYBOARD)
        {
        }

        protected override int HookProc(int nCode, int wParam, IntPtr lParam)
        {
            //����������в����û�Ҫ�������̵���Ϣ  
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
