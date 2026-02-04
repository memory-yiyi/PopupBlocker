using System.IO;

namespace PopupBlocker.Core
{
    public static class AppPath
    {
        private static readonly string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static readonly string _appFolder = Path.Combine(_appDataPath, "PopupBlocker");
        private static readonly string _ruleConfigFilePath = Path.Combine(_appFolder, RuleConfigFileName);

        static AppPath()
        {
            Directory.CreateDirectory(_appFolder);
        }

        public const string RuleConfigFileName = "PopupBlocker_RuleConfig.json";
        public static string RuleConfigFilePath => _ruleConfigFilePath;
    }
}
