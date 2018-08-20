using MultipleQueueDispatcher.Helper;
using MultipleQueueDispatcher.Queue;
using MultipleQueueDispatcher.Worker;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MultipleQueueDispatcher.Incoming
{
    public class DispatcherLogic<T, TQueueId> : IDispatcherLogic<T, TQueueId>
    {
        private readonly Func<T, TQueueId> _predicateToCreateNewInternalQ;
        private readonly IWorkerManager<T> _workerManager;
        private readonly ITime _time;
        private ConcurrentDictionary<TQueueId, QueueWithIncrementingSequenceNumbers<T>> _qDictionary = new ConcurrentDictionary<TQueueId, QueueWithIncrementingSequenceNumbers<T>>();

        public DispatcherLogic(
                    Func<T, TQueueId> predicateToCreateNewInternalQ,
            IWorkerManager<T> workerManager,
            ITime time
            )
        {
            _predicateToCreateNewInternalQ = predicateToCreateNewInternalQ;
            _workerManager = workerManager;
            _time = time;
        }

        public Func<T, TQueueId> GetQueueId => _predicateToCreateNewInternalQ;

        public int NrOfInternalQueues { get => _qDictionary.Count; }

        public bool AreAllQueuesEmpty() => _qDictionary.All(kvp => kvp.Value.IsEmpty);

        public void PerformQueueActions()
        {
            foreach (var kvp in _qDictionary)
            {
                if (kvp.Value.TryDequeue(out var item))
                {
                    _workerManager.HandleNewItem(item);
                }
            }
        }

        public void DeleteQueue(TQueueId id) => _qDictionary.TryRemove(id, out var _);

        public void HandleNewItem(T item)
        {
            var key = GetQueueId(item);
            if (!_qDictionary.ContainsKey(key))
            {
                // We need to create a new Q
                _qDictionary[key] = new QueueWithIncrementingSequenceNumbers<T>(_time, TimeSpan.FromSeconds(5));
            }
            _qDictionary[key].Enqueue(item);
        }

        public bool IsEmpty(TQueueId queueId) => _qDictionary.ContainsKey(queueId) && _qDictionary[queueId].IsEmpty;
    }
}