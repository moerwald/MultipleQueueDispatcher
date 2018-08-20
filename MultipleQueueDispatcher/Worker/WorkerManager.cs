using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MultipleQueueDispatcher.Worker
{
    public interface IWorkerManager<T>
    {
        void HandleNewItem(T item);
    }

    public class WorkerManager<T> : IWorkerManager<T>
    {
        private int _nrOfWorkers;
        private int _lastWorkerIndex = 0;

        public WorkerManager(int nrOfWorkers, CancellationTokenSource cts, Action<T> actionToPerform)
        {
            _nrOfWorkers = nrOfWorkers;
            var workerFactory = new WorkerFactory<T>(cts);
            foreach (var i in Enumerable.Range(0, _nrOfWorkers)) { _workerCollection[i] = workerFactory.Create(actionToPerform, i); }
        }

        public void HandleNewItem(T item) => _workerCollection[_lastWorkerIndex % _nrOfWorkers].Run(item);

        private List<IWorker<T>> _workerCollection = new List<IWorker<T>>();
    }
}
