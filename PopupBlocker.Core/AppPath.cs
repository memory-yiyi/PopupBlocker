using System.IO;

namespace PopupBlocker.Core
{
    public static class AppPath
    {
        private static readonly string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string _appFolder = Path.Combine(_appDataPath, AppName);
        private static readonly string _ruleConfigFilePath = Path.Combine(_appFolder, RuleConfigFileName);
        private static readonly string _settingFilePath = Path.Combine(_appFolder, SettingFileName);

        static AppPath()
        {
            Directory.CreateDirectory(_appFolder);
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            ExecutingPath = process.MainModule!.FileName;
        }

        public const string RuleConfigFileName = "PopupBlocker_RuleConfig.json";
        public static string RuleConfigFilePath => _ruleConfigFilePath;
        public const string SettingFileName = "PopupBlocker_Setting.json";
        public static string SettingFilePath => _settingFilePath;

        public const string AppName = "PopupBlocker";
        public const string AutoRunSwitchProperty = "/autorun";
        public static string ExecutingPath { get; }
    }
}
