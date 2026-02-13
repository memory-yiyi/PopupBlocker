using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace PopupBlocker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            /* 并不是说Debug模式不需要管理员权限
             * 只是提供一个快速定位bug在不在核心的方法
             * 没有管理员权限的程序，核心一定不会工作
             */
#if !DEBUG
            // 检查是否以管理员身份运行
            if (!IsRunningAsAdministrator())
            {
                // 直接通过UAC请求管理员权限
                var processInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath,
                    UseShellExecute = true,
                    Verb = "runas" // 这会触发Windows UAC提示框
                };

                try
                {
                    Process.Start(processInfo)?.Dispose();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    /* 依伊的低语
                     * ai 还是太温柔了，这种小逝干嘛要叨扰用户（doge
                     */
                    // 用户拒绝UAC请求或启动失败，直接退出
                    //MessageBox.Show("程序需要管理员权限才能正常运行。", "需要管理员权限", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // 关闭当前实例
                Current.Shutdown();
                return;
            }
#endif
            // 正常启动逻辑
            base.OnStartup(e);

#if true    // 上面的空间用于临时测试，有需要记得改为false
            new Views.Tray(e.Args.Length == 0 || e.Args[0] != Core.AppPath.AutoRunSwitchProperty).Show();
#endif
        }

        private static bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
