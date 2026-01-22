using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

namespace PopupBlocker.Core.Services
{
    public sealed class ThreadMonitorService : Utility.Interfaces.StatusManagerBase
    {
        public ThreadMonitorService() { }

        #region StatusManagerBase
        public override bool IsRunning => _session is not null;
        public override bool IsStopped => _session is null;

        protected override void OnStart()
        {
            // 创建ETW会话
            _session = new TraceEventSession("ThreadMonitorSession", null)
            {
                StopOnDispose = true    // 设置为true，以便在会话对象被销毁时停止ETW会话（其实默认就是true，表强调）
            };
            // 启用内核提供程序的线程事件
            _session.EnableKernelProvider(KernelTraceEventParser.Keywords.Thread);
            // 订阅线程创建事件
            _session.Source.Kernel.ThreadStart += OnThreadCreated;
            // 创建后台线程以处理线程创建事件
            _processingThread = new Thread(() => _session.Source.Process())
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "ThreadMonitorService"
            };
            // 启动ETW会话
            _processingThread.Start();
        }

        protected override void OnStop()
        {
            _session!.Dispose(); // 得益于StopOnDispose为true，所以Dispose时会调用Stop方法，从而停止会话并释放资源
            _processingThread!.Join(1000);
            _processingThread = null;
            _session = null;
        }
        #endregion

        #region ETW线程监控
        /* 为什么要使用ETW而不是循环检测或钩子？
         * 循环检测会占用大量CPU资源，不是每时每刻都有广告，显然非常浪费资源
         * 全局钩子？抛开系统崩溃不谈，广告应用在所有应用中占比没你想的那么多，全局钩子在浪费资源
         * 线程钩子？抛开广告会开新进程不谈，你还要实时监控所有线程的创建并挂钩，这比循环检测还浪费资源
         */
        private TraceEventSession? _session;
        private Thread? _processingThread;

        private void OnThreadCreated(ThreadTraceData data)
        {

        }
        #endregion
    }
}
