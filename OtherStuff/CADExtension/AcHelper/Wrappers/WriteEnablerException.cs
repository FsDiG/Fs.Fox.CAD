using System;

namespace AcHelper.Wrappers
{
    [Serializable]
    public class WriteEnablerException : Exception
    {
        public WriteEnablerException() { }
        public WriteEnablerException(string message) : base(message) { }
        public WriteEnablerException(string message, Exception inner) : base(message, inner) { }
        protected WriteEnablerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
