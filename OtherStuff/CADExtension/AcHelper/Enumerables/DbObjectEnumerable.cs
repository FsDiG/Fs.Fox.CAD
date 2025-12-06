using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcHelper.Enumerables
{
    /// <summary>
    /// An <c>IEnumerable</c> implementation that returns a <see cref="DbObjectEnumerator{T}"/>.
    /// </summary>
    /// <typeparam name="T">A type that derives from <c>DBObject</c>.</typeparam>
    public class DbObjectEnumerable<T> : IEnumerable<T> where T : DBObject
    {
        private readonly IEnumerable<ObjectId> _enumerable;
        private readonly OpenMode _openMode;
        private readonly Transaction _transaction;

        /// <summary>
        /// Initializes a new _instance of the <see cref="DbObjectEnumerable{T}"/> class.
        /// </summary>
        /// <param name="enumerable">The enumerable</param>
        /// <param name="transaction">The current transaction.</param>
        /// <param name="openMode">The open mode.</param>
        public DbObjectEnumerable(IEnumerable<ObjectId> enumerable, Transaction transaction, OpenMode openMode)
        {
            _enumerable = enumerable;
            _transaction = transaction;
            _openMode = openMode;
        }

        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used
        /// to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new DbObjectEnumerator<T>(_enumerable.GetEnumerator(), _transaction, _openMode);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used
        /// to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
