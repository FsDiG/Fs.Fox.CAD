using System;

namespace AcHelper.Wrappers
{
    [Serializable]
    public class AcTransactionException : Exception
    {
        public AcTransactionException() { }
        public AcTransactionException(string message) : base(message) { }
        public AcTransactionException(string message, Exception inner) : base(message, inner) { }
        protected AcTransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
