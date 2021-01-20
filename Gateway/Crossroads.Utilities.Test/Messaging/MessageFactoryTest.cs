using System.Messaging;
using Crossroads.Utilities.Messaging;
using Moq;
using NUnit.Framework;

namespace Crossroads.Utilities.Test.Messaging
{
    public class MessageFactoryTest
    {
        private MessageFactory _fixture;
        private Mock<IMessageFormatter> _messageFormatter;

        [SetUp]
        public void SetUp()
        {
            _fixture = new MessageFactory();
            _messageFormatter = new Mock<IMessageFormatter>();
        }

        [Test]
        public void TestCreateMessage()
        {
            var msg = _fixture.CreateMessage("body", _messageFormatter.Object);
            Assert.IsNotNull(msg);
            Assert.AreEqual("body", msg.Body);
            Assert.AreEqual(_messageFormatter.Object, msg.Formatter);
        }

        [Test]
        public void TestCreateMessageNoFormatter()
        {
            var msg = _fixture.CreateMessage("body", null);
            Assert.IsNotNull(msg);
            Assert.IsNotNull(msg.Formatter);
            Assert.AreEqual(typeof(JsonMessageFormatter), msg.Formatter.GetType());
        }

    }
}
