using System;
using System.IO;

namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class AcadDirectory : IEquatable<AcadDirectory>
    {
        /// <summary>
        /// The _directory information
        /// </summary>
        private readonly DirectoryInfo _directoryInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcadDirectory"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public AcadDirectory(string path)
        {
            _directoryInfo = path.StartsWith("%") ? new DirectoryInfo(Environment.ExpandEnvironmentVariables(path)) : new DirectoryInfo(path);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AcadDirectory"/> is exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if exists; otherwise, <c>false</c>.
        /// </value>
        public bool Exists { get { return _directoryInfo.Exists; } }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get { return _directoryInfo.FullName; } }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return _directoryInfo.Name; } }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(AcadDirectory other)
        {
            if (other == null)
                return false;

            return this.Name == other.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var acadDir = obj as AcadDirectory;
            return acadDir != null && Equals(acadDir);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (String.IsNullOrEmpty(Name) ? Name.GetHashCode() : 0);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="acadDir1">The acad dir1.</param>
        /// <param name="acadDir2">The acad dir2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(AcadDirectory acadDir1, AcadDirectory acadDir2)
        {
            if ((object)acadDir1 == null || ((object)acadDir2) == null)
                return Equals(acadDir1, acadDir2);

            return acadDir1.Equals(acadDir2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="acadDir1">The acad dir1.</param>
        /// <param name="acadDir2">The acad dir2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(AcadDirectory acadDir1, AcadDirectory acadDir2)
        {
            if (acadDir1 == null || acadDir2 == null)
                return !Equals(acadDir1, acadDir2);

            return !(acadDir1.Equals(acadDir2));
        }
    }
}