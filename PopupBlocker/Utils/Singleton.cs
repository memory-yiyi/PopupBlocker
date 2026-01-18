namespace PopupBlocker.Utils
{
    public class Singleton<T> where T : class, new()
    {
        // 相较于??运算符Lazy<T>无需判断_instance是否为null
        // 直接返回实例，可以减少系统开销

        private static readonly Lazy<T> _instance = new(() => new T());
        public static T Instance => _instance.Value;
    }
}
