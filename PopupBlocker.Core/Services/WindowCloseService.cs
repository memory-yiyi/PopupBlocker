using PopupBlocker.Core.Models;
using PopupBlocker.Utility.Commons;
using PopupBlocker.Utility.Windows;

namespace PopupBlocker.Core.Services
{
    public class WindowCloseService(BlockRules rules)
    {
        private static ServiceManager ServiceManager => Singleton<ServiceManager>.Instance;
        private readonly BlockRules _rules = rules;
        private readonly WindowInfo _windowInfo = new();
        private readonly object _lock = new();
        private byte _loopCount;
        private const int _loopCountMax = 5;

        public void CheckAndBlockWindows()
        {
            if (_loopCount <= 1)
                Task.Run(() =>
                {
                    for (_loopCount = _loopCountMax; _loopCount > 0; --_loopCount)
                    {
                        Thread.Sleep(30);
                        lock (_lock)
                        {
                            WinAPI.EnumWindows(EnumWindowCallback, IntPtr.Zero);
                        }
                    }
                });
            else
                _loopCount = _loopCountMax;
        }

        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private bool EnumWindowCallback(UIntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == UIntPtr.Zero || !WinAPI.IsWindowVisible(hWnd))
                    return true;

                _windowInfo.Handle = hWnd;
                var processName = _windowInfo.ProcessName;

                if (processName != _rules!.ProcessName)
                    return true;

                var className = _windowInfo.WindowClass;
                var windowTitle = _windowInfo.WindowTitle;

                ServiceManager.CurrentLoggerService.Debug($"检查窗口：{processName} - {className} - {windowTitle}");
                var rule = RuleConfigService.FindRule(_rules, className, windowTitle);
                if (rule is null)
                    return true;
                ServiceManager.CurrentLoggerService.Info($"拦截窗口：{processName} - {className} - {windowTitle}");

                CloseWindowSafely(hWnd);
                ServiceManager.CurrentRuleConfigService.AddRuleCount(rule);
            }
            catch (Exception ex)
            {
                ServiceManager.CurrentLoggerService.Warning($"检查窗口时出错：{ex.Message}");
            }
            return true;
        }

        private void CloseWindowSafely(UIntPtr hWnd)
        {
            try
            {
                // 首先尝试优雅关闭
                WinAPI.PostMessage(hWnd, WinAPI.WM_CLOSE, UIntPtr.Zero, IntPtr.Zero);

                Thread.Sleep(50);

                // 如果窗口仍然存在，强制销毁
                if (WinAPI.IsWindowVisible(hWnd))
                    WinAPI.DestroyWindow(hWnd);
            }
            catch (Exception ex)
            {
                ServiceManager.CurrentLoggerService.Debug($"关闭窗口时出错：{ex.Message}");
                // 最后手段：隐藏窗口
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
        }
    }
}
