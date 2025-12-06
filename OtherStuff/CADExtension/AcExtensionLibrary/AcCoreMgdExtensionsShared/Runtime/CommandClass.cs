using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Autodesk.AutoCAD.Runtime
{
    /// <summary>
    /// Just refactored into own class from
    /// Author: Kerry Brown
    /// Source: http://www.theswamp.org/index.php?topic=37686.msg427172#msg427172
    /// </summary>
    public abstract class CommandClass : ICommandClass
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public Document Doc { get; private set; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public Database Db { get; private set; }

        /// <summary>
        /// Gets the ed.
        /// </summary>
        /// <value>
        /// The ed.
        /// </value>
        public Editor Ed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandClass"/> class.
        /// </summary>
        public CommandClass()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Db = Doc.Database;
            Ed = Doc.Editor;
        }
    }
}