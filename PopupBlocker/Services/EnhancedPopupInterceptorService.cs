using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PopupBlocker.Models;

namespace PopupBlocker.Services
{
    public class EnhancedPopupInterceptorService : IDisposable
    {
        private bool _isRunning;
        private Thread? _monitorThread;
        private CancellationTokenSource? _cancellationTokenSource;
        
        private readonly List<InterceptorRule> _rules = new();
        private readonly ConfigService _configService = new();

        // Windows API 常量
        private const int SW_HIDE = 0;

        // Windows API 函数
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public event Action<string>? OnPopupBlocked;

        public EnhancedPopupInterceptorService()
        {
            LoadRules();
        }

        public void LoadRules()
        {
            _rules.Clear();
            _rules.AddRange(_configService.LoadRules());
        }

        public void SaveRules()
        {
            _configService.SaveRules(_rules);
        }

        public List<InterceptorRule> GetAllRules()
        {
            return new List<InterceptorRule>(_rules);
        }

        public void AddRule(InterceptorRule rule)
        {
            _rules.Add(rule);
            SaveRules();
        }

        public void RemoveRule(InterceptorRule rule)
        {
            _rules.RemoveAll(r => r.Type == rule.Type && r.Pattern == rule.Pattern);
            SaveRules();
        }

        public void UpdateRule(InterceptorRule rule)
        {
            var existingRule = _rules.FirstOrDefault(r => r.Type == rule.Type && r.Pattern == rule.Pattern);
            if (existingRule != null)
            {
                existingRule.Enabled = rule.Enabled;
                existingRule.Description = rule.Description;
                SaveRules();
            }
        }

        public void ExportRules(string filePath)
        {
            _configService.ExportRules(_rules, filePath);
        }

        public void ImportRules(string filePath)
        {
            var importedRules = _configService.ImportRules(filePath);
            _rules.Clear();
            _rules.AddRange(importedRules);
            SaveRules();
        }

        public void ResetAllCounts()
        {
            foreach (var rule in _rules)
            {
                rule.BlockedCount = 0;
                rule.LastBlockedTime = null;
            }
            SaveRules();
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            _monitorThread = new Thread(MonitorWindows);
            _monitorThread.IsBackground = true;
            _monitorThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _monitorThread?.Join(1000);
        }

        private void MonitorWindows()
        {
            while (_isRunning)
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
                    Debug.WriteLine($"监控线程异常: {ex.Message}");
                }
            }
        }

        private void CheckAndBlockWindows()
        {
            EnumWindows(EnumWindowCallback, IntPtr.Zero);
        }

        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !IsWindowVisible(hWnd))
                    return true;

                // 获取窗口信息
                var titleBuilder = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
                var title = titleBuilder.ToString();

                var classBuilder = new System.Text.StringBuilder(256);
                GetClassName(hWnd, classBuilder, classBuilder.Capacity);
                var className = classBuilder.ToString();

                GetWindowThreadProcessId(hWnd, out var processId);
                var process = Process.GetProcessById((int)processId);
                var processName = process.ProcessName;

                // 检查所有启用的规则
                foreach (var rule in _rules.Where(r => r.Enabled))
                {
                    if (MatchesRule(rule, processName, className, title))
                    {
                        rule.IncrementBlockCount();
                        SaveRules(); // 保存计数到JSON
                        CloseWindowSafely(hWnd);
                        OnPopupBlocked?.Invoke($"拦截: {rule.Type} - {rule.Pattern} ({title})");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理窗口时出错: {ex.Message}");
            }

            return true;
        }

        private bool MatchesRule(InterceptorRule rule, string processName, string className, string title)
        {
            return rule.Type switch
            {
                RuleType.Process => processName.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                RuleType.WindowClass => className.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                RuleType.WindowTitle => title.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        /// <summary>
        /// 安全关闭窗口
        /// </summary>
        private void CloseWindowSafely(IntPtr hWnd)
        {
            try
            {
                // 首先尝试优雅关闭
                PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                
                Thread.Sleep(50);
                
                // 如果窗口仍然存在，强制销毁
                if (IsWindowVisible(hWnd))
                {
                    DestroyWindow(hWnd);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"关闭窗口时出错: {ex.Message}");
                // 最后手段：隐藏窗口
                ShowWindow(hWnd, SW_HIDE);
            }
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}