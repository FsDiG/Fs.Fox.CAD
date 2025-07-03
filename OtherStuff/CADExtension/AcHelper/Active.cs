using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.IO;

namespace AcHelper
{
    /// <summary>
    /// runtime environment.
    /// </summary>
    public static class Active
    {
        /// <summary>
        /// Returns the active Document object.
        /// </summary>
        public static Document Document
        {
            get { return Application.DocumentManager.MdiActiveDocument; }
        }
        /// <summary>
        /// Returns the active Database object.
        /// </summary>
        public static Database Database
        {
            get { return Document.Database; }
        }
        /// <summary>
        /// Returns the active Database object.
        /// </summary>
        public static Editor Editor
        {
            get { return Document.Editor; }
        }

        /// <summary>
        /// Returns the Name of the active document.
        /// </summary>
        public static string DocumentName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(Document.Name);
            }
        }
        /// <summary>
        /// Gets the Directory of the active document.
        /// </summary>
        public static string DocumentDirectory
        {
            get
            {
                return new FileInfo(Document.Name).Directory.FullName;
            }
        }
        /// <summary>
        /// Returns Full path of the active document.
        /// </summary>
        public static string DocumentFullPath
        {
            get
            {
                return new FileInfo(Document.Name).FullName;
            }
        }

        /// <summary>
        /// Sends a string to the command line in the active Editor.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public static void WriteMessage(string message)
        {
            Editor.WriteMessage("\n" + message);
        }
        /// <summary>
        /// Sends a string to the command line in the active Editor using String.Format.
        /// </summary>
        /// <param name="message">The message containing format specifications.</param>
        /// <param name="parameters">The variables to substitue into the format string.</param>
        public static void WriteMessage(string message, params object[] parameters)
        {
            Editor.WriteMessage("\n" + message, parameters);
        }
		public static void WriteMessageInLine(string message)
		{
			Editor.WriteMessage(message);
		}
		public static void WriteMessageInLine(string message, params object[] parameters)
		{
			Editor.WriteMessage(message, parameters);
		}
        /// <summary>
        /// Regenerates active modelspace. Equivalent of Editor.Regen().
        /// </summary>
        public static void Regenerate()
        {
            Editor.Regen();
        }
    }
}
