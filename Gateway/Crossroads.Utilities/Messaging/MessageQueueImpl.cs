using System.Messaging;
using Crossroads.Utilities.Messaging.Interfaces;

namespace Crossroads.Utilities.Messaging
{
    public class MessageQueueImpl : IMessageQueue
    {
        private readonly IMessageQueueFactory _messageQueueFactory;
        private MessageQueue _messageQueue;

        public MessageQueueImpl(IMessageQueueFactory messageQueueFactory)
        {
            _messageQueueFactory = messageQueueFactory;
        }
        public void Send(object message, MessageQueueTransactionType type)
        {
            _messageQueue.Send(message, type);
        }

        public MessageQueue CreateQueue(string queueName, QueueAccessMode accessMode, IMessageFormatter formatter = null)
        {
            _messageQueue = _messageQueueFactory.CreateQueue(queueName, accessMode, formatter);
            // If sending messages, make the queue durable
            if (accessMode == QueueAccessMode.Send || accessMode == QueueAccessMode.SendAndReceive)
            {
                _messageQueue.DefaultPropertiesToSend.Recoverable = true;
            }

            return _messageQueue;
        }

        public bool Exists(string path)
        {
            return MessageQueue.Exists(path);
        }

        public MessageQueue Create(string path)
        {
            _messageQueue = MessageQueue.Create(path);
            return _messageQueue;
        }
    }
}
