using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
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
