using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Fpi.Util.WinApiUtil
{
    /// <summary>
    /// WinForm相关WinApi函数包装类
    /// </summary>
    public class WinFrmAboutApiWrapper
    {
        /// <summary>
        /// DLL
        /// </summary>
        private const string DLL_NAME = "user32.dll";

        #region API

        /// <summary> 隐藏光标
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static bool HideCaret(IntPtr hWnd)
        {
            return WinHideCaret(hWnd);
        }

        /// <summary> 调用一个窗口的窗口函数，将一条消息命令发给那个窗口
        /// </summary>
        /// <param name="hwnd">要接收消息的那个窗口的句柄</param>
        /// <param name="wMsg">消息的标识符</param>
        /// <param name="wParam"></param>
        /// <param name="IParam"></param>
        /// <returns></returns>
        public static bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int IParam)
        {
            return sendMessage(hwnd, wMsg, wParam, IParam);
        }

        /// <summary> 为当前的应用程序释放鼠标的捕获
        /// </summary>
        /// <returns></returns>
        public static bool ReleaseCapture()
        {
            return releaseCapture();
        }

        /// <summary> 窗体动画效果  
        /// <para>AW_HOR_POSITIVE = 0x00000001：自左向右显示窗口。该标志可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略。</para>
        /// <para>AW_HOR_NEGATIVE = 0x00000002：自右向左显示窗口。该标志可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略。</para>
        /// <para>AW_VER_POSITIVE = 0x00000004：自顶向下显示窗口。该标志可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略。</para>
        /// <para>AW_VER_NEGATIVE = 0x00000008：自下向上显示窗口。该标志可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略。</para>
        /// <para>AW_CENTER = 0x00000010：若使用了AW_HIDE标志，则使窗口向内重叠；若未使用AW_HIDE标志，则使窗口向外扩展。</para>
        /// <para>AW_HIDE = 0x00010000：隐藏窗口，缺省则显示窗口。</para>
        /// <para>AW_ACTIVATE = 0x00020000：激活窗口。在使用了AW_HIDE标志后不要使用这个标志。</para>
        /// <para>AW_SLIDE = 0x00040000：使用滑动类型。缺省则为滚动动画类型。当使用AW_CENTER标志时，这个标志就被忽略。</para>
        /// <para>AW_BLEND = 0x00080000：使用淡出效果。只有当hWnd为顶层窗口的时候才可以使用此标志。</para>
        /// </summary>
        /// <param name="handle"> hWnd：指定产生动画的窗口的句柄。</param>
        /// <param name="ms">dwTime：指明动画持续的时间（以微秒计），完成一个动画的标准时间为200微秒。</param>
        /// <param name="flags">dwFags：指定动画类型。这个参数可以是一个或多个下列标志的组合。</param>
        /// <returns></returns>
        public static bool AnimateWindow(IntPtr handle, int ms, int flags)
        {
            return animateWindow(handle, ms, flags);
        }

        #endregion

        #region  API imports

        /// <summary> 隐藏光标
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport(DLL_NAME, EntryPoint = "HideCaret", SetLastError = true)]
        private static extern bool WinHideCaret(IntPtr hWnd);

        /// <summary> 调用一个窗口的窗口函数，将一条消息命令发给那个窗口
        /// </summary>
        /// <returns></returns>
        [DllImport(DLL_NAME, EntryPoint = "SendMessage", SetLastError = true)]
        private static extern bool sendMessage(IntPtr hwnd, int wMsg, int wParam, int IParam);

        /// <summary> 为当前的应用程序释放鼠标的捕获
        /// </summary>
        /// <returns></returns>
        [DllImport(DLL_NAME, EntryPoint = "ReleaseCapture", SetLastError = true)]
        private static extern bool releaseCapture();

        /// <summary> 窗体动画效果
        /// </summary>
        [DllImport(DLL_NAME, EntryPoint = "AnimateWindow", SetLastError = true)]
        private static extern bool animateWindow(IntPtr handle, int ms, int flags);

        #endregion
    }
}
