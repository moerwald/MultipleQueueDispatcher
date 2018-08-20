using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace MultipleQueueDispatcher.Worker
{

    public interface IWorker<T>
    {
        void Run(T item);
    }

    public class Worker<T>:  IWorker<T>
    {
        private readonly Action<T> _action;
        private readonly ActionBlock<T> _ab;
        private readonly int _id;

        public Worker(Action<T> action, int workerId, CancellationToken token)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _ab = new ActionBlock<T>(action, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1, CancellationToken = token });
            _id = workerId;
        }

        public void Run(T item) => _action(item);

        public override string ToString() => $" Worker {_id}";
    }
}
