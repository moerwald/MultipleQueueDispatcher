using MultipleQueueDispatcher.Incoming;
using MultipleQueueDispatcher.Worker;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using Moq;

namespace MultipleQueueDispatcher.Test.NetFramework
{
    [TestFixture]
    public class DispatcherLogicTest
    {
        private CancellationTokenSource _cts;
        private Mock<IWorkerManager<int>> _workerMangerMock;
        private DispatcherLogic<int, int> _logic;

        [SetUp]
        public void Setup()
        {
            _cts = new CancellationTokenSource();
            _workerMangerMock = new Mock<IWorkerManager<int>>();
            _logic = new DispatcherLogic<int, int>(i => i % 3, _workerMangerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
        }

        [Test]
        public void NrOfInternalQueues_Shall_Be_3()
        {
            foreach(var i in Enumerable.Range(0,9))
            {
                _logic.HandleNewItem(i);
            }
            Assert.AreEqual(3, _logic.NrOfInternalQueues);
        }


        [Test]
        public void TestMethod()
        {
            foreach(var i in Enumerable.Range(0,9))
            {
                _logic.HandleNewItem(i);
            }

            _logic.PerformQueueActions();

            _workerMangerMock.Verify(wm => wm.HandleNewItem(It.IsInRange(0, 9, Range.Exclusive)), Times.Exactly(9));
        }
    }
}
