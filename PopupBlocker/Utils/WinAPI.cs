using System.Runtime.InteropServices;

namespace PopupBlocker.Utils
{
    public static class WinAPI
    {
        #region 常量
        public const int SW_HIDE = 0;
        public const uint WM_CLOSE = 0x0010;
        #endregion

        #region 结构
        public delegate bool EnumWindowsProc(nint hWnd, nint lParam);
        #endregion

        #region 方法
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(nint hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern int GetClassName(nint hWnd, System.Text.StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(nint hWnd);
        #endregion
    }
}
