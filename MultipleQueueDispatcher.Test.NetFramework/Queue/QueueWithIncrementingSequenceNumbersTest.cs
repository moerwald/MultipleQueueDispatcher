using Moq;
using MultipleQueueDispatcher.Helper;
using MultipleQueueDispatcher.Queue;
using NUnit.Framework;
using System;

namespace MultipleQueueDispatcher.Test.NetFramework.Queue
{
    [TestFixture]
    public class QueueWithIncrementingSequenceNumbersTest
    {
        private Mock<ITime> _time;
        private QueueWithIncrementingSequenceNumbers<int> _q;

        [SetUp]
        public void Setup()
        {
            _time = new Mock<ITime>();
            _q = new QueueWithIncrementingSequenceNumbers<int>(_time.Object, TimeSpan.FromSeconds(3));
        }

        [Test]
        public void TryDequeue_EntryIsValid_ReturnEntry()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime);
            _q.Enqueue(1);
            Assert.IsTrue(_q.TryDequeue(out var entry));
            Assert.AreEqual(1, entry);
        }

        [Test]
        public void TryDequeue_EntryIsToOld_ReturnFalse()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime);
            _q.Enqueue(1);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime.AddSeconds(5));
            Assert.IsFalse(_q.TryDequeue(out var _));
        }

        [Test]
        public void TryPeek_EntryIsValid_ReturnEntry()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime);
            _q.Enqueue(1);
            Assert.IsTrue(_q.TryPeek(out var entry));
            Assert.AreEqual(1, entry);
        }

        [Test]
        public void TryPeek_EntryIsToOld_ReturnFalse()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime);
            _q.Enqueue(1);
            _time.Setup(time => time.GetUtcNow()).Returns(startTime.AddSeconds(5));
            Assert.IsFalse(_q.TryPeek(out var _));
        }

    }
}