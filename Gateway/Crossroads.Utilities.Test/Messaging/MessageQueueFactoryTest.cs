using System.Messaging;
using Crossroads.Utilities.Messaging;
using Moq;
using NUnit.Framework;

namespace Crossroads.Utilities.Test.Messaging
{
    public class MessageQueueFactoryTest
    {
        private MessageQueueFactory _fixture;
        private Mock<IMessageFormatter> _messageFormatter;

        [SetUp]
        public void SetUp()
        {
            _fixture = new MessageQueueFactory();
            _messageFormatter = new Mock<IMessageFormatter>();
        }

        [Test]
        public void TestCreateQueue()
        {
            var queue = _fixture.CreateQueue("name", QueueAccessMode.Peek, _messageFormatter.Object);
            Assert.IsNotNull(queue);
            Assert.AreEqual("name", queue.QueueName);
            Assert.AreEqual(QueueAccessMode.Peek, queue.AccessMode);
            Assert.AreEqual(_messageFormatter.Object, queue.Formatter);
            var filter = queue.MessageReadPropertyFilter;
            Assert.IsTrue(filter.Body);
            Assert.IsTrue(filter.ArrivedTime);
        }

        [Test]
        public void TestCreateQueueNoMessageFormatter()
        {
            var queue = _fixture.CreateQueue("name", QueueAccessMode.Peek, null);
            Assert.IsNotNull(queue);
            Assert.IsNotNull(queue.Formatter);
            Assert.AreEqual(typeof(JsonMessageFormatter), queue.Formatter.GetType());
        }
    }
}
