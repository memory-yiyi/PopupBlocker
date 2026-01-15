using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PopupBlocker.Models;
using PopupBlocker.Services;

namespace PopupBlocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EnhancedPopupInterceptorService? _interceptorService;
        private int _blockCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeInterceptorService();
        }

        private void InitializeInterceptorService()
        {
            _interceptorService = new EnhancedPopupInterceptorService();
            _interceptorService.OnPopupBlocked += OnPopupBlocked;
            RefreshRulesList();
        }

        private void OnPopupBlocked(string message)
        {
            Dispatcher.Invoke(() =>
            {
                _blockCount++;
                RefreshRulesList(); // 更新规则计数
                tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                tbLog.ScrollToEnd();
            });
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _interceptorService?.Start();
                tbStatus.Text = "状态: 运行中";
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 拦截器已启动\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _interceptorService?.Stop();
                tbStatus.Text = "状态: 已停止";
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 拦截器已停止\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestPopupWindow.ShowTestPopup();
                tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 测试弹窗已显示\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddRule_Click(object sender, RoutedEventArgs e)
        {
            var pattern = txtRulePattern.Text.Trim();
            if (string.IsNullOrEmpty(pattern))
            {
                MessageBox.Show("请输入规则模式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ruleType = cmbRuleType.SelectedIndex switch
            {
                0 => RuleType.Process,
                1 => RuleType.WindowClass,
                2 => RuleType.WindowTitle,
                _ => RuleType.WindowTitle
            };

            var ruleTypeName = cmbRuleType.SelectedItem.ToString() ?? "未知";

            try
            {
                var rule = new InterceptorRule(ruleType, pattern, true, $"用户添加的{ruleTypeName}规则");
                _interceptorService?.AddRule(rule);
                txtRulePattern.Clear();
                RefreshRulesList();
                tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 添加{ruleTypeName}规则: {pattern}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveRule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is InterceptorRule rule)
            {
                try
                {
                    _interceptorService?.RemoveRule(rule);
                    RefreshRulesList();
                    tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 移除规则: {rule.Type} - {rule.Pattern}\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"移除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnToggleRule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is InterceptorRule rule)
            {
                try
                {
                    rule.Enabled = checkBox.IsChecked ?? false;
                    _interceptorService?.UpdateRule(rule);
                    RefreshRulesList();
                    tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 规则已{(rule.Enabled ? "启用" : "禁用")}: {rule.Type} - {rule.Pattern}\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"更新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    FileName = "popup_blocker_config.json",
                    Title = "导出配置"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    _interceptorService?.ExportRules(saveFileDialog.FileName);
                    tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 配置已导出到: {saveFileDialog.FileName}\n");
                    MessageBox.Show("配置导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    Title = "导入配置"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show("导入配置将覆盖当前所有规则，是否继续？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _interceptorService?.ImportRules(openFileDialog.FileName);
                        RefreshRulesList();
                        tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 配置已从 {openFileDialog.FileName} 导入\n");
                        MessageBox.Show("配置导入成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnResetCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("确定要将所有拦截计数清零吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _interceptorService?.ResetAllCounts();
                    RefreshRulesList();
                    tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 所有拦截计数已清零\n");
                    MessageBox.Show("计数已重置！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshRulesList()
        {
            lvRules.Items.Clear();
            if (_interceptorService != null)
            {
                var rules = _interceptorService.GetAllRules();
                foreach (var rule in rules)
                {
                    lvRules.Items.Add(rule);
                }
                
                // 更新状态栏
                var totalBlocked = rules.Sum(r => r.BlockedCount);
                var enabledCount = rules.Count(r => r.Enabled);
                tbStatusInfo.Text = $"规则: {rules.Count} (启用: {enabledCount}) | 总拦截: {totalBlocked}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _interceptorService?.Dispose();
            base.OnClosed(e);
        }
    }
}