using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Autodesk.AutoCAD.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public interface ICommandClass
    {
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        Document Doc { get; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        Database Db { get; }

        /// <summary>
        /// Gets the ed.
        /// </summary>
        /// <value>
        /// The ed.
        /// </value>
        Editor Ed { get; }
    }
}