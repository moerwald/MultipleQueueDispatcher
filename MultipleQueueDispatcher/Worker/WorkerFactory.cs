using System;
using System.Threading;

namespace MultipleQueueDispatcher.Worker
{
    public interface IWorkerFactory<T>
    {
        IWorker<T> Create(Action<T> action, int wokerId);
    }

    public class WorkerFactory<T> : IWorkerFactory<T>
    {
        public WorkerFactory(CancellationTokenSource cts) => _cts = cts;
        private CancellationTokenSource _cts  ;
        public IWorker<T> Create(Action<T> action, int workerId) => new Worker<T>(action, workerId, _cts.Token);
    }
}
