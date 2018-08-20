using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MultipleQueueDispatcher.Incoming
{
    public class DispatcherLogicHost<T, TQueueId> : IDisposable
    {
        public ActionBlock<T> _inputQ;
        private readonly Task _dispatcherTask;
        private CancellationTokenSource _cts;
        private IDispatcherLogic<T, TQueueId> _logic;

        public DispatcherLogicHost(
            CancellationTokenSource cts,
            IDispatcherLogic<T, TQueueId> logic)
        {
            // Parameter checks
            _cts = cts ?? throw new ArgumentNullException(nameof(cts));
            _logic = logic;

            // Create attributes
            _dispatcherTask = new Task(() => Schedule(_cts.Token));
            _dispatcherTask.Start();
            _inputQ = new ActionBlock<T>(
                item => _logic.HandleNewItem(item), 
                new ExecutionDataflowBlockOptions { CancellationToken = cts.Token, MaxDegreeOfParallelism = 1 });
        }

        public void Push(T item) => _inputQ.Post(item);

        private void Schedule(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (_logic.AreAllQueuesEmpty())
                {
                    YieldCpuToOtherThread();
                }
                else
                {
                    _logic.PerformQueueActions();
                }
            }

            void YieldCpuToOtherThread()
            {
                if (!Thread.Yield())
                {
                    // Avoid busy waiting
                    Thread.Sleep(50);
                }
            }
        }

        #region IDisposable Support

        private volatile bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _cts.Cancel();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IncomingDispatcher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        #endregion IDisposable Support
    }
}