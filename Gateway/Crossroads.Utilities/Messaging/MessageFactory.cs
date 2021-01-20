using System.Messaging;
using Crossroads.Utilities.Messaging.Interfaces;

namespace Crossroads.Utilities.Messaging
{
    public class MessageFactory : IMessageFactory
    {
        public Message CreateMessage(dynamic messageBody, IMessageFormatter formatter)
        {
            return (new Message(messageBody, formatter ?? new JsonMessageFormatter())
            {
                Recoverable = true
            });
        }
    }
}
