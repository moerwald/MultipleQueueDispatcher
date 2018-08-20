using Moq;
using MultipleQueueDispatcher.Helper;
using MultipleQueueDispatcher.Queue;
using NUnit.Framework;
using System;
using System.Linq;

namespace MultipleQueueDispatcher.Test.NetFramework.Queue
{
    [TestFixture]
    public class QueueWithIncrementingSequenceNumbersTest
    {
        private Mock<ITime> _time;
        private QueueWithIncrementingSequenceNumbers<int> _q;
        private DateTime _refereneTime;

        [SetUp]
        public void Setup()
        {
            _time = new Mock<ITime>();
            _q = new QueueWithIncrementingSequenceNumbers<int>(_time.Object, TimeSpan.FromSeconds(3));
            _refereneTime = new DateTime(1970, 1, 1, 0, 0, 0);
            _time.Setup(time => time.GetUtcNow()).Returns(_refereneTime);
        }

        [Test]
        public void TryDequeue_EntryIsValid_ReturnEntry()
        {
            Enumerable.Range(0, 100).ToList().ForEach(i => _q.Enqueue(i));
            Enumerable.Range(0, 100).ToList().ForEach(i =>
            {
                Assert.IsTrue(_q.TryDequeue(out var entry));
                Assert.AreEqual(i, entry);
            });
        }

        [Test]
        public void TryDequeue_EntryIsToOld_ReturnFalse()
        {
            _time.Setup(time => time.GetUtcNow()).Returns(_refereneTime.AddSeconds(5));
            Assert.IsFalse(_q.TryDequeue(out var _));
        }

        [Test]
        public void TryPeek_EntryIsValid_ReturnEntry()
        {
            Enumerable.Range(0, 100).ToList().ForEach(i => _q.Enqueue(i));
            Enumerable.Range(0, 100).ToList().ForEach(i =>
            {
                Assert.IsTrue(_q.TryPeek(out var entry));
                Assert.AreEqual(i, entry);
                Assert.IsTrue(_q.TryDequeue(out var _));
            });
        }

        [Test]
        public void TryPeek_EntryIsToOld_ReturnFalse()
        {
            _q.Enqueue(1);
            _time.Setup(time => time.GetUtcNow()).Returns(_refereneTime.AddSeconds(5));
            Assert.IsFalse(_q.TryPeek(out var _));
        }


    }
}