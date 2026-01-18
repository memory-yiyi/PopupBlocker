using System.Diagnostics;
using PopupBlocker.Models;
using PopupBlocker.Utils;

namespace PopupBlocker.Services
{
    public class PopupInterceptorService : IDisposable
    {
        #region 服务启动与停止逻辑
        private Thread? _monitorThread;
        private CancellationTokenSource? _cts;
        private readonly LoggerService _logger = Singleton<LoggerService>.Instance;
        public PopupInterceptorService() { }

        public void Start()
        {
            if (IsRunning)
                return;

            _cts = new CancellationTokenSource();
            _monitorThread = new Thread(MonitorWindows)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "PopupInterceptorMonitor"
            };
            _monitorThread.Start();
            _logger.Info("拦截器已启动");
        }

        public void Stop()
        {
            if (IsStopped)
                return;

            _cts!.Cancel();
            _monitorThread?.Join(1000);
            _cts.Dispose();
            _cts = null;
            _logger.Info("拦截器已停止");
        }

        public bool IsRunning => _cts is not null;
        public bool IsStopped => _cts is null;
        #endregion

        #region 监控逻辑
        private readonly System.Text.StringBuilder _titleBuilder = new(256);
        private readonly System.Text.StringBuilder _classBuilder = new(256);
        private readonly ConfigService _config = Singleton<ConfigService>.Instance;

        private void MonitorWindows()
        {
            while (!_cts!.IsCancellationRequested)
            {
                try
                {
                    CheckAndBlockWindows();
                    Thread.Sleep(100); // 每100ms检查一次
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"监控线程异常: {ex.Message}");
                }
            }
        }

        private void CheckAndBlockWindows()
        {
            WinAPI.EnumWindows(EnumWindowCallback, IntPtr.Zero);
        }

        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !WinAPI.IsWindowVisible(hWnd))
                    return true;

                // 获取窗口信息
                _ = WinAPI.GetWindowText(hWnd, _titleBuilder, _titleBuilder.Capacity);
                var title = _titleBuilder.ToString();

                _ = WinAPI.GetClassName(hWnd, _classBuilder, _classBuilder.Capacity);
                var className = _classBuilder.ToString();

                _ = WinAPI.GetWindowThreadProcessId(hWnd, out var processId);
                var process = Process.GetProcessById((int)processId);
                var processName = process.ProcessName;

                // 检查所有启用的规则
                foreach (var rule in _config.Where(r => r.Enabled))
                {
                    if (MatchesRule(rule, processName, className, title))
                    {
                        _config.IncrementRuleCount(rule); // 更新规则计数
                        CloseWindowSafely(hWnd);
                        _logger.Info($"拦截: {rule.Type} - {rule.Pattern} ({title})");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"检查窗口时出错: {ex.Message}");
            }
            return true;
        }

        private static bool MatchesRule(InterceptorRule rule, string processName, string className, string title)
        {
            return rule.Type switch
            {
                RuleType.Process => processName.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                RuleType.WindowClass => className.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                RuleType.WindowTitle => title.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        private static void CloseWindowSafely(IntPtr hWnd)
        {
            try
            {
                // 首先尝试优雅关闭
                WinAPI.PostMessage(hWnd, WinAPI.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                Thread.Sleep(50);

                // 如果窗口仍然存在，强制销毁
                if (WinAPI.IsWindowVisible(hWnd))
                    WinAPI.DestroyWindow(hWnd);
            }
            catch (Exception ex)
            {
                Singleton<LoggerService>.Instance.Debug($"关闭窗口时出错: {ex.Message}");
                // 最后手段：隐藏窗口
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
        }
        #endregion

        #region 释放模式
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    Stop();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PopupInterceptorService()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
