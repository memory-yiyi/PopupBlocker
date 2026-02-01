using PopupBlocker.Utility.Commons;
using PopupBlocker.Utility.Windows;
using System.Diagnostics;

namespace PopupBlocker.Core.Services
{
    public class PopupInterceptorService : Utility.Interfaces.ETWThreadMonitor
    {
        public PopupInterceptorService() { }
        private readonly LoggerService _logger = Singleton<LoggerService>.Instance;

        #region 服务启动与停止逻辑
        protected override void OnStart()
        {
            base.OnStart();
            _logger.Info("拦截器已启动");
        }

        protected override void OnStop()
        {
            base.OnStop();
            _logger.Info("拦截器已停止");
        }
        #endregion

        #region 监控逻辑
        [ThreadStatic]
        private static System.Text.StringBuilder? _titleBuilder;
        [ThreadStatic]
        private static System.Text.StringBuilder? _classBuilder;
        [ThreadStatic]
        private static string? _processName;
        private readonly ConfigService _config = Singleton<ConfigService>.Instance;

        protected override void OnThreadCreated(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ThreadTraceData data)
        {
            if (_config.ExistRules(data.ProcessName))
                Task.Run(() => CheckAndBlockWindows(data.ProcessName));
        }

        private void CheckAndBlockWindows(string processName)
        {
            _titleBuilder = new System.Text.StringBuilder(256);
            _classBuilder = new System.Text.StringBuilder(256);
            _processName = processName;
            WinAPI.EnumWindows(EnumWindowCallback, IntPtr.Zero);
        }

        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private bool EnumWindowCallback(UIntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == UIntPtr.Zero || !WinAPI.IsWindowVisible(hWnd))
                    return true;

                _ = WinAPI.GetWindowThreadProcessId(hWnd, out var processId);
                using var process = Process.GetProcessById((int)processId);
                var processName = process.ProcessName;

                if (processName != _processName)
                    return true;

                _ = WinAPI.GetClassName(hWnd, _classBuilder!, _classBuilder!.Capacity);
                var className = _classBuilder.ToString();

                _ = WinAPI.GetWindowText(hWnd, _titleBuilder!, _titleBuilder!.Capacity);
                var title = _titleBuilder.ToString();

                _logger.Debug($"检查窗口：{processName} - {className} - {title}");

                if (!_config.MatchRule(processName, className))
                    return true;

                CloseWindowSafely(hWnd);
            }
            catch (Exception ex)
            {
                _logger.Warning($"检查窗口时出错：{ex.Message}");
            }
            return true;
        }

        private static void CloseWindowSafely(UIntPtr hWnd)
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
                Singleton<LoggerService>.Instance.Debug($"关闭窗口时出错：{ex.Message}");
                // 最后手段：隐藏窗口
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
        }
        #endregion
    }
}
