using PopupBlocker.Utility.Windows;

namespace PopupBlocker.Utility.Commons
{
    public static class WindowInfo
    {
        [ThreadStatic]
        private static System.Text.StringBuilder? _classBuilder;
        [ThreadStatic]
        private static System.Text.StringBuilder? _titleBuilder;

        public static string GetWindowThreadProcessName(UIntPtr handle)
        {
            _ = WinAPI.GetWindowThreadProcessId(handle, out var processId);
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }

        public static string GetWindowClass(UIntPtr handle)
        {
            _classBuilder ??= new System.Text.StringBuilder(256);
            _ = WinAPI.GetClassName(handle, _classBuilder, _classBuilder.Capacity);
            var className = _classBuilder.ToString();
            if (className.StartsWith("HwndWrapper"))
                return className[12..className.IndexOf(';', 12)];
            else
                return className;
        }

        public static string GetWindowTitle(UIntPtr handle)
        {
            _titleBuilder ??= new System.Text.StringBuilder(256);
            _ = WinAPI.GetWindowText(handle, _titleBuilder, _titleBuilder.Capacity);
            return _titleBuilder.ToString();
        }

        public static bool IsMainWindowHandle(UIntPtr handle)
        {
            _ = WinAPI.GetWindowThreadProcessId(handle, out var processId);
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return (UIntPtr)process.MainWindowHandle == handle;
        }
    }
}
