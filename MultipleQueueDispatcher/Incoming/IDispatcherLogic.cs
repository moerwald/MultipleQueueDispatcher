using MultipleQueueDispatcher.Worker;
using System;

namespace MultipleQueueDispatcher.Incoming
{
    public interface IDispatcherLogic<T, TQueueId>
    {
        Func<T, TQueueId> GetQueueId { get; }

        int NrOfInternalQueues { get; }

        bool AreAllQueuesEmpty();

        void PerformQueueActions();

        void DeleteQueue(TQueueId id);

        void HandleNewItem(T item);

        bool IsEmpty(TQueueId queueId);
    }
}