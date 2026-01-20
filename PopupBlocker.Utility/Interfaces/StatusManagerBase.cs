namespace PopupBlocker.Utility.Interfaces
{
    public abstract class StatusManagerBase : IDisposable
    {
        public abstract bool IsRunning { get; }
        public abstract bool IsStopped { get; }

        protected abstract void OnStart();
        protected abstract void OnStop();

        public void Start()
        {
            if (IsStopped)
                OnStart();
        }
        public void Stop()
        {
            if (IsRunning)
                OnStop();
        }

        #region IDisposable
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
        // ~StatusManagerBase()
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
    }
    #endregion
}
