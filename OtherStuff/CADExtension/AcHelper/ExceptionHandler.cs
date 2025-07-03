using System;
using System.Text;

namespace AcHelper
{
    /// <summary>
    /// contains extensions for Exceptions to either:
    /// print the exception on the commandline,
    /// show dialog with the exception,
    /// both print and display a dialog.
    /// </summary>
    public static class ExceptionHandler
    {
        private const string START_EXCEPTION = "***  Exception  ***";
        private const string END_EXCEPTION = "*** End Exception ***";

        /// <summary>
        /// Log, display dialog, and write in commandline.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="priority">Severity of the exception.</param>
        public static void ProcessException(this Exception ex)
        {
            ProcessException(ex, true, true);
        }
        /// <summary>
        /// Log the exception.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="priority">Severity of the exception.</param>
        /// <param name="commandLine">True if you want to write the exception on the commandline.</param>
        /// <param name="dialog">True if you want to display a dialog with the exception message(s).</param>
        public static void ProcessException(this Exception ex, bool commandLine = false, bool dialog = false)
        {
            if (commandLine)
            {
                WriteToCommandLine(ex);
            }
            if (dialog)
            {
                ShowDialog(ex);
            }
        }

        /// <summary>
        /// Writes the exception message(s) on the commandline.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public static void WriteToCommandLine(this Exception ex)
        {
            WriteToCommandLine(ex, true);
        }
        /// <summary>
        /// Writes the exception message(s) on the commandline.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <param name="inner">True if messages from innerexceptions also need to be written on the commandline.</param>
        public static void WriteToCommandLine(this Exception ex, bool inner)
        {
            StartException();

            Active.WriteMessage(ex.Message);

            if (inner)
            {
                Active.WriteMessage(GetInnerExceptions(ex));
            }

            EndException();
        }

        /// <summary>
        /// Displays the exception message(s) on a dialog.
        /// </summary>
        /// <param name="ex"></param>
        public static void ShowDialog(this Exception ex)
        {
            ShowDialog(ex, true);
        }

        /// <summary>
        /// Displays the exception message in a dialog.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="inner">true if messages from innerexceptions also need to be displayed.</param>
        /// 
        public static void ShowDialog(this Exception ex, bool inner)
        {
            StringBuilder sb = new StringBuilder(ex.Message);

            if (inner)
            {
                sb.Append("\n" + GetInnerExceptions(ex));
            }

            Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(sb.ToString());
        }

        #region Private Methods ...
        private static string GetInnerExceptions(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            Exception inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendFormat("{0}\n", inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }
        private static void StartException()
        {
            Active.WriteMessage(START_EXCEPTION);
        }
        private static void EndException()
        {
            Active.WriteMessage(END_EXCEPTION);
        } 
        #endregion
    }
}
