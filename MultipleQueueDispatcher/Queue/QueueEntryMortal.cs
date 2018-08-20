using System;

namespace MultipleQueueDispatcher.Queue
{
    public class QueueEntryMortal<T>
    {
        public QueueEntryMortal(T entry, DateTime birthTime, TimeSpan timeToLive)
        {
            Entry = entry;
            BirthTime = birthTime;
            TimeToLive = timeToLive;
        }

        public DateTime BirthTime { get; }
        public DateTime CalculatedTimeToLive { get => BirthTime.Add(TimeToLive); }
        public T Entry { get; }
        public TimeSpan TimeToLive { get; }
    }
}