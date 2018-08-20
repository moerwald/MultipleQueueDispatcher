using MultipleQueueDispatcher.Worker;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MultipleQueueDispatcher.Queue;
using MultipleQueueDispatcher.Helper;

namespace MultipleQueueDispatcher.Incoming
{
    public class IncomingDispatcher<T, TQueueId> : IDisposable
    {
        public ActionBlock<T> _inputQ;

        private readonly Task _dispatcherTask;

        private readonly IWorkerManager<T> _workerManager;

        private CancellationTokenSource _cts;

        private Func<T, TQueueId> _getQueueId;

        private ConcurrentDictionary<TQueueId, QueueWithIncrementingSequenceNumbers<T>> _qDictionary = new ConcurrentDictionary<TQueueId, QueueWithIncrementingSequenceNumbers<T>>();

        public IncomingDispatcher(Func<T, TQueueId> predicateToCreateNewInternalQ,
            CancellationTokenSource cts,
            IWorkerManager<T> workerManager)
        {
            // Parameter checks
            _cts = cts ?? throw new ArgumentNullException(nameof(cts));
            _getQueueId = predicateToCreateNewInternalQ ?? throw new ArgumentNullException(nameof(predicateToCreateNewInternalQ));
            _workerManager = workerManager ?? throw new ArgumentNullException(nameof(workerManager));

            // Create attributes
            var token = cts.Token;
            _dispatcherTask = new Task(() => Schedule(_cts.Token));
            _inputQ = new ActionBlock<T>(item => HandleNewItem(item), new ExecutionDataflowBlockOptions { CancellationToken = cts.Token });
        }

        public int NrOfInternalQueues { get => _qDictionary.Count; }

        public void DeleteQueue(TQueueId id) => _qDictionary.TryRemove(id, out var _);
        public bool IsEmpty(TQueueId queueId) => _qDictionary.ContainsKey(queueId) && _qDictionary[queueId].IsEmpty;

        public void Push(T item) => _inputQ.Post(item);

        private void HandleNewItem(T item)
        {
            var key = _getQueueId(item);
            if (!_qDictionary.ContainsKey(key))
            {
                // We need to create a new Q
                _qDictionary[key] = new QueueWithIncrementingSequenceNumbers<T>(new Time(), TimeSpan.FromSeconds(5));
            }
            _qDictionary[key].Enqueue(item);
        }

        public void StartScheduling() => _dispatcherTask.Start();

        private void Schedule(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (_qDictionary.Values.All(q => q.IsEmpty))
                {
                    // All qs empty -> YieldCpu
                    YieldCpu();
                }
                else
                {
                    // "Round robin" over every q
                    foreach (var kvp in _qDictionary)
                    {
                        if (kvp.Value.TryDequeue(out var item))
                        {
                            _workerManager.HandleNewItem(item);
                        }
                    }
                }
            }

            void YieldCpu()
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