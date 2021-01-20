using System.Messaging;

namespace Crossroads.Utilities.Messaging.Interfaces
{
    public interface IMessageFactory
    {
        Message CreateMessage(dynamic messageBody, IMessageFormatter formatter = null);
    }
}
