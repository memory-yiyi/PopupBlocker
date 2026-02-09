using System.Windows;
using System.Windows.Input;
using static PopupBlocker.Utility.Windows.WinAPI;

namespace PopupBlocker.Controls
{
    public class SelectorWindow : Window
    {
        public SelectorWindow()
        {
            var progman = FindWindow("Progman", null);
            var desktop = FindWindowEx(
                FindWindowEx(progman, UIntPtr.Zero, "SHELLDLL_DefView", null),
                UIntPtr.Zero, "SysListView32", null);
            var taskbar = FindWindow("Shell_TrayWnd", null);
            var taskbarSwitch = FindWindowEx(
                FindWindowEx(taskbar, UIntPtr.Zero, "ReBarWindow32", null),
                UIntPtr.Zero, "MSTaskSwWClass", null);
            _handleBlackList = [desktop, taskbar, taskbarSwitch];
            _dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);
            Loaded += (_, _) => StartMouseFollow();
        }

        protected override void OnClosed(EventArgs e)
        {
            _isActive = false;
            _mouseMoveTask?.Wait(2000);
            base.OnClosed(e);
        }

        #region 鼠标跟随窗口
        public UIntPtr GetWindowHandleFromMouse()
        {
            _isActive = false;
            _mouseMoveTask!.Wait();
            _mouseMoveTask = null;
            return Handle;
        }

        public void StartMouseFollow()
        {
            _isActive = true;
            _mouseMoveTask ??= Task.Run(SetWindowRectFromMouse);
        }

        private bool _isActive;
        private Task? _mouseMoveTask;
        private DpiScale _dpi;
        protected UIntPtr Handle { get; private set; }
        private readonly List<UIntPtr> _handleBlackList;
        private POINT _point;

        private class Point
        {
            public POINT WinPoint;
            private const int Range = 100;   // 手别抖，OK？
            public bool Equals(POINT point) => WinPoint.X >= point.X - Range && WinPoint.X <= point.X + Range && WinPoint.Y >= point.Y - Range && WinPoint.Y <= point.Y + Range;
        }
        private readonly Point _lastPoint = new();
        private class Rect
        {
            public RECT WinRect;
            public double Left => WinRect.Left;
            public double Top => WinRect.Top;
            public double Width => WinRect.Right - WinRect.Left;
            public double Height => WinRect.Bottom - WinRect.Top;
            //public bool Contains(POINT point) => WinRect.Left <= point.X && WinRect.Top <= point.Y && WinRect.Right >= point.X && WinRect.Bottom >= point.Y;
        }
        private readonly Rect _lastRect = new();

        private void SetWindowRectFromMouse()
        {
            while (_isActive)
            {
                GetCursorPos(out _lastPoint.WinPoint);
                if (_lastPoint.Equals(_point))
                    continue;

                _point = _lastPoint.WinPoint;

                var handle = WindowFromPoint(_point);
                if (handle == UIntPtr.Zero || handle == Handle)
                    continue;
                if (_handleBlackList.Contains(handle))
                    continue;
                if (!IsWindowVisible(handle))
                    continue;

                this.Dispatcher?.Invoke(() =>
                {
                    Left = 0;
                    Top = 0;
                    Width = 0;
                    Height = 0;
                });
                Handle = handle;
                GetWindowRect(handle, out _lastRect.WinRect);

                this.Dispatcher?.Invoke(() =>
                {
                    Left = _lastRect.Left / _dpi.DpiScaleX;
                    Top = _lastRect.Top / _dpi.DpiScaleY;
                    Width = _lastRect.Width / _dpi.DpiScaleX;
                    Height = _lastRect.Height / _dpi.DpiScaleY;
                });

                Thread.Sleep(1000);
            }
        }
        #endregion

        static SelectorWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SelectorWindow),
                new FrameworkPropertyMetadata(typeof(SelectorWindow)));
        }

        public static readonly DependencyProperty WindowClickProperty =
            DependencyProperty.Register(
                nameof(WindowClick),
                typeof(ICommand),
                typeof(SelectorWindow),
                new PropertyMetadata(null, OnWindowClickChanged));

        public ICommand WindowClick
        {
            get => (ICommand)GetValue(WindowClickProperty);
            set => SetValue(WindowClickProperty, value);
        }

        private static void OnWindowClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SelectorWindow selector)
                selector.MouseDown += (_, _) => ((ICommand)d.GetValue(WindowClickProperty)).Execute(selector);
        }
    }
}
