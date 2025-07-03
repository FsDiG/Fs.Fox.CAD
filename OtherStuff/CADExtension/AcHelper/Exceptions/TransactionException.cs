using System;

namespace AcHelper.Exceptions
{
    [Serializable]
    public class TransactionException : Exception
    {
        private string _command_name;
        public string CommandName
        {
            get { return _command_name; }
        }

        public TransactionException(string message, string commandName)
            : base (message)
        {
            _command_name = commandName;
        }
        public TransactionException(string message, string commandName, Exception inner)
            : base(message, inner)
        {
            _command_name = commandName;
        }

        #region Exception members ...
        public TransactionException() { }
        public TransactionException(string message) : base(message) { }
        public TransactionException(string message, Exception inner) : base(message, inner) { }
        protected TransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
        #endregion
    }
}
