using System;

namespace AcHelper.Commands
{
	[Serializable]
	public class SendStringToExecuteException : ArgumentException
	{
		const string MESSAGE = "Couldn't execute command from the CommandLine.";
		private string _command;
		public SendStringToExecuteException() : base(MESSAGE) { }
		public SendStringToExecuteException(string command) : base(MESSAGE, command) { _command = command; }
		public SendStringToExecuteException(string message, Exception inner) : base(message, inner) { }
		public SendStringToExecuteException(string command, string message, Exception inner) : base(message, command, inner) { }
		public SendStringToExecuteException(string command, string message) : base (message, command) { _command = command; }
		protected SendStringToExecuteException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		public string Command => _command;
	}
}
