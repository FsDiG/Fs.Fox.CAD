using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace AcHelper.Enumerables
{
    /// <summary>
    /// Generic <c>IEnumerator</c> implementation that wraps an <c>IEnumerator&lt;ObjectId&gt;</c>
    /// and instead gives actual objects that derive from <c>DBObject</c>.
    /// </summary>
    /// <typeparam name="T">A type that derives from <c>DBObject</c>.</typeparam>
    public class DbObjectEnumerator<T> : IEnumerator<T> where T : DBObject
    {
        private readonly IEnumerator<ObjectId> _enumerator;
        private readonly OpenMode _open_mode;
        private readonly RXClass _ent_type;
        private readonly Transaction _transaction;

        /// <summary>
        /// Initializes a new _instance of the <see cref="DbObjectEnumerator{T}"/> class.
        /// </summary>
        /// <param name="enumerator">The enumerator to wrap.</param>
        /// <param name="transaction">The current transaction.</param>
        /// <param name="openMode">The open mode.</param>
        public DbObjectEnumerator(IEnumerator<ObjectId> enumerator, Transaction transaction, OpenMode openMode)
        {
            _enumerator = enumerator;
            _transaction = transaction;
            _open_mode = openMode;
            _ent_type = RXObject.GetClass(typeof(T));
        }

        #region IEnumerator<T> Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element;
        /// false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.ObjectClass.IsDerivedFrom(_ent_type))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first
        /// element in the collection
        /// </summary>
        public void Reset()
        {
            _enumerator.Reset();
        }


        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        /// The element in the collection at the current position of the enumerator. 
        /// </returns>
        public T Current
        {
            get { return (T)_transaction.GetObject(_enumerator.Current, _open_mode); }
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        object IEnumerator.Current
        {
            get { return Current; }
        }
        #endregion
    }
}
