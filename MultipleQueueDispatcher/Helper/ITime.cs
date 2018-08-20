using System;

namespace MultipleQueueDispatcher.Helper
{
    public interface ITime
    {
        DateTime GetUtcNow();
    }


    public class Time : ITime
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
