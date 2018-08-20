using MultipleQueueDispatcher.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MultipleQueueDispatcher.Queue
{
    /// <summary>
    /// Decorator for ConcurrentQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueWithIncrementingSequenceNumbers<T>
    {
        private readonly ITime _time;

        private readonly TimeSpan _timeToLive;

        private ConcurrentQueue<QueueEntryMortal<T>> _q = new ConcurrentQueue<QueueEntryMortal<T>>();

        public QueueWithIncrementingSequenceNumbers(ITime time, TimeSpan timeToLive)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _timeToLive = timeToLive;
        }

        public int Count { get => _q.Count; }
        public bool IsEmpty { get => _q.IsEmpty; }

        public void CopyTo(T[] array, int index)
        {
                var lst = array.ToList().Select(itm => new QueueEntryMortal<T>(itm, _time.GetUtcNow(), _timeToLive)).ToArray();
                _q.CopyTo(lst, index);
        }

        public void Enqueue(T item)
        {
                _q.Enqueue(new QueueEntryMortal<T>(item, _time.GetUtcNow(), _timeToLive));
        }

        public IEnumerator<QueueEntryMortal<T>> GetEnumerator() => _q.GetEnumerator();

        public QueueEntryMortal<T>[] ToArray() => _q.ToArray();

        public bool TryDequeue(out T result)
        {
            result = default(T);
            while (_q.TryDequeue(out var entry))
            {
                var now = _time.GetUtcNow();
                if (entry.CalculatedTimeToLive < now)
                {
                    // Entry died -> get next one
                    continue;
                }
                result = entry.Entry;
                return true;
            }
            return false;
        }

        public bool TryPeek(out T result)
        {
            while (_q.TryPeek(out var entry))
            {
                if (entry.CalculatedTimeToLive < _time.GetUtcNow())
                {
                    // Entry died -> remove it
                    _q.TryDequeue(out var _);
                    continue;
                }

                result = entry.Entry;

                return true;
            }

            result = default(T);
            return false;
        }
    }
}