namespace PopupBlocker.Utility.Interfaces
{
    public enum Status { Init, Active, Run, Pause, Stop, Changing }

    public abstract class StatusManager : IDisposable
    {
        protected virtual void OnInit() { }
        protected virtual void OnStart() { }
        protected virtual void OnPause() { }
        protected virtual void OnContinue() { }
        protected virtual void OnStop() { }
        protected virtual void OnEnd() { }

        private Status _status = Status.Init;
        public Status Status => _status;

        public void Init()
        {
            if (_status == Status.Init)
            {
                _status = Status.Changing;
                OnInit();
                _status = Status.Active;
            }
        }

        public void Start()
        {
            Init();
            if (_status == Status.Active)
            {
                _status = Status.Changing;
                OnStart();
                _status = Status.Run;
            }
        }

        public void Pause()
        {
            if (_status == Status.Run)
            {
                _status = Status.Changing;
                OnPause();
                _status = Status.Pause;
            }
        }

        public void Continue()
        {
            if (_status == Status.Pause)
            {
                _status = Status.Changing;
                OnContinue();
                _status = Status.Run;
            }
        }

        public void Stop(bool isEnd = true)
        {
            Continue();
            if (_status == Status.Run)
            {
                _status = Status.Changing;
                OnStop();
                _status = Status.Stop;
            }
            if (isEnd && _status == Status.Stop)
            {
                _status = Status.Changing;
                OnEnd();
                _status = Status.Init;
            }
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
