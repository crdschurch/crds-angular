using System;
using System.Messaging;

namespace Crossroads.Utilities.Messaging.Interfaces
{
    public interface IMessageQueue
    {
        void Send(Object message, MessageQueueTransactionType type);
        MessageQueue CreateQueue(string queueName, QueueAccessMode accessMode, IMessageFormatter formatter = null);
        bool Exists(string path);
        MessageQueue Create(string path);
    }
}
