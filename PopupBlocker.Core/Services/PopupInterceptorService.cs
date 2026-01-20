using PopupBlocker.Core.Models;
using PopupBlocker.Utility.Commons;
using PopupBlocker.Utility.Windows;
using System.Diagnostics;

namespace PopupBlocker.Core.Services
{
    public class PopupInterceptorService : Utility.Interfaces.StatusManagerBase
    {
        #region 服务启动与停止逻辑
        private Thread? _monitorThread;
        private CancellationTokenSource? _cts;
        private readonly LoggerService _logger = Singleton<LoggerService>.Instance;
        public PopupInterceptorService() { }


        public override bool IsRunning => _cts is not null;
        public override bool IsStopped => _cts is null;

        protected override void OnStart()
        {
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

        protected override void OnStop()
        {
            _cts!.Cancel();
            _monitorThread?.Join(1000);
            _cts.Dispose();
            _cts = null;
            _logger.Info("拦截器已停止");
        }
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

        private bool EnumWindowCallback(UIntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == UIntPtr.Zero || !WinAPI.IsWindowVisible(hWnd))
                    return true;

                // 获取窗口信息
                _ = WinAPI.GetWindowText(hWnd, _titleBuilder, _titleBuilder.Capacity);
                var title = _titleBuilder.ToString();

                _ = WinAPI.GetClassName(hWnd, _classBuilder, _classBuilder.Capacity);
                var className = _classBuilder.ToString();

                _ = WinAPI.GetWindowThreadProcessId(hWnd, out var processId);
                using var process = Process.GetProcessById((int)processId);
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
                Singleton<LoggerService>.Instance.Debug($"关闭窗口时出错: {ex.Message}");
                // 最后手段：隐藏窗口
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
        }
        #endregion
    }
}
