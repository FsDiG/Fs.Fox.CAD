using System;
using System.Text;

namespace AcHelper.Commands
{
    /// <summary>
    /// The CommandHandlerBase provides base functionalities for the class containing AutoCAD commands. 
    /// </summary>
    public abstract class CommandHandlerBase
    {
        private const string WHITESPACE = " ";
        private const string NEWLINE = "\n";
        private const string ESCAPECHARACTERS = "\x03\x03";

        /// <summary>
        /// Executes the given <see cref="IAcadCommand"/>IAcadCommand class as a command.
        /// When an unhandled exception occurs, it will be caught and thrown as a dialog.
        /// This way the the chance for an AutoCAD crash stays minimal.
        /// </summary>
        /// <typeparam name="T">Class based on interface IAcadCommand</typeparam>
        public static void ExecuteCommand<T>() where T : IAcadCommand
        {
            try
            {
                // Create an instance of the command class
                var cmd = Activator.CreateInstance<T>();
                cmd.Execute();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowDialog(ex, true);
            }
        }
        /// <summary>
        /// Executes a command from the commandline.
        /// </summary>
        /// <param name="cmd">Command name.</param>
        /// <param name="parameters">Optional parameters.</param>
        public static void ExecuteFromCommandLine(string cmd, params object[] parameters)
        {
            ExecuteFromCommandLine(true, cmd, parameters);
        }
        /// <summary>
        /// Executes a command from the commandline.
        /// </summary>
        /// <param name="echo">true to echo command in commandline.</param>
        /// <param name="cmd">Command name.</param>
        /// <param name="parameters">Optional parameters.</param>
        public static void ExecuteFromCommandLine(bool echo, string cmd, params object[] parameters)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                throw new ArgumentNullException("cmd");
            }

            // Prepare Command and potential parameters.
            StringBuilder executeBuilder = new StringBuilder(cmd);
            executeBuilder.Insert(0, ESCAPECHARACTERS);
            executeBuilder.Append(NEWLINE);
            string execute = cmd;
            execute += NEWLINE;
            // Check for potential parameters
            if (parameters != null && parameters.Length > 0)
            {
                // if parameters are present
                // add every parameter to the command.
                foreach (var item in parameters)
                {
                    // end parameter with a whitespace so it will be executed.
                    execute += item.ToString() + WHITESPACE;
                    executeBuilder.Append(item.ToString());
                    executeBuilder.Append(WHITESPACE);
                }
            }

            try
            {
                // Execute
                // Active.Document.SendStringToExecute(execute, true, false, echo);
                Active.Document.SendStringToExecute(executeBuilder.ToString(), true, false, echo);
            }
            catch (Exception ex)
            {
                SendStringToExecuteException e = new SendStringToExecuteException(cmd, ex);
                ExceptionHandler.ShowDialog(e, true);
            }
        }
    }
}
