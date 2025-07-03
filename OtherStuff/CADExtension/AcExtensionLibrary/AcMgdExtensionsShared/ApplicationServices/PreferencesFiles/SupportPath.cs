using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class SupportPath : IAcadPathRepository
    {
        /// <summary>
        /// The seperator
        /// </summary>
        private static char[] seperator = new char[] { ';' };

        /// <summary>
        /// The paths
        /// </summary>
        private List<string> paths = new List<string>();

        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        /// <value>
        /// The preferences.
        /// </value>
        protected dynamic Preferences { get; set; }

        /// <summary>
        /// Gets or sets the preferences file.
        /// </summary>
        /// <value>
        /// The preferences file.
        /// </value>
        protected virtual dynamic PreferencesFile
        {
            get { return Preferences.Files.SupportPath; }
            set { Preferences.Files.SupportPath = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public SupportPath(object acadPreferences)
        {
            Preferences = acadPreferences;
            paths = CreatepathList(PreferencesFile);
        }

        /// <summary>
        /// Gets the paths.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetPaths()
        {
            return paths.AsReadOnly();
        }

        /// <summary>
        /// Adds the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Add(string path)
        {
            path = Expand(path);
            paths.Add(path);
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="path">The path.</param>
        public void Insert(int index, string path)
        {
            path = Expand(path);
            paths.Insert(index, path);
        }

        /// <summary>
        /// Removes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool Remove(string path)
        {
            path = Expand(path);
            int index = paths.FindIndex(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
            paths.RemoveAt(index);
            return index > -1;
        }

        /// <summary>
        /// Removes at.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            paths.RemoveAt(index);
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int RemoveAll(Predicate<string> match)
        {
            return paths.RemoveAll(match);
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public void RemoveRange(int index, int count)
        {
            paths.RemoveRange(index, count);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            paths.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool Contains(string path)
        {
            return paths.Contains(Expand(path), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        public void SaveChanges()
        {
            this.PreferencesFile = CreatePathsString(paths);
            paths = CreatepathList(PreferencesFile);
        }

        /// <summary>
        /// Createpathes the list.
        /// </summary>
        /// <param name="pathString">The path string.</param>
        /// <returns></returns>
        private static List<string> CreatepathList(string pathString)
        {
            return pathString.Split(seperator).ToList();
        }

        /// <summary>
        /// Creates the paths string.
        /// </summary>
        /// <param name="pathList">The path list.</param>
        /// <returns></returns>
        private static string CreatePathsString(IEnumerable<string> pathList)
        {
            return String.Join(";", pathList);
        }

        /// <summary>
        /// Expands the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string Expand(string path)
        {
            return path.StartsWith("%") ? Environment.ExpandEnvironmentVariables(path) : path;
        }
    }
}