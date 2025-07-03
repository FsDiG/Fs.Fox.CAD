using System.Collections.Generic;

namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    internal interface IAcadPathRepository
    {
        /// <summary>
        /// Gets the paths.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetPaths();

        /// <summary>
        /// Adds the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        void Add(string path);

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="path">The path.</param>
        void Insert(int index, string path);

        /// <summary>
        /// Removes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        bool Remove(string path);

        /// <summary>
        /// Saves the changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Determines whether [contains] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        bool Contains(string path);
    }
}