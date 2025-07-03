using System;

namespace AcHelper
{
    public class LoggerNotInitializedException : Exception
    {
        private const string MESSAGE = "Logger instance is not initialized.\nIn the IExtensionApplication.Initialize() method, pass a LogSetup into Logger.InitializeLogger(LogSetup) method.";
        #region Exception members ...
        public LoggerNotInitializedException() : base(MESSAGE) { }
        public LoggerNotInitializedException(string message) : base(message) { }
        public LoggerNotInitializedException(string message, Exception inner) : base(message, inner) { }
        protected LoggerNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
        #endregion
    }
}
