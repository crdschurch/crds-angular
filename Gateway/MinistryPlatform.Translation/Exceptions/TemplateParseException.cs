using System;
using System.Runtime.Serialization;

namespace MinistryPlatform.Translation.Exceptions
{
    [Serializable]
    public class TemplateParseException : Exception
    {

        public TemplateParseException()
        {
        }

        public TemplateParseException(string message) : base(message)
        {
        }

        public TemplateParseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TemplateParseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
