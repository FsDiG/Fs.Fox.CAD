using System;
using System.Collections;
using System.Collections.Generic;
using AcRuntimeInterop = Autodesk.AutoCAD.Runtime.Interop;

namespace Autodesk.AutoCAD.DatabaseServices.Wrapper
{
    /// <summary>
    /// A generic wrapper class for wrapping the 9 common dictionaries created by AutoCAD. Group, Layout, etc.......
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WrapperDBDictionary<T> : DBDictionary, IEnumerable<T> where T : DBObject
    {
        /// <summary>
        /// Gets or sets the trans.
        /// </summary>
        /// <value>
        /// The trans.
        /// </value>
        private Transaction trans { get; set; }
        /// <summary>
        /// The m_including erased
        /// </summary>
        private bool m_includingErased;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperDBDictionary{T}"/> class.
        /// </summary>
        /// <param name="trx">The TRX.</param>
        /// <param name="dic">The dic.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        internal WrapperDBDictionary(Transaction trx, DBDictionary dic, bool includingErased)
            : base(dic.UnmanagedObject, dic.AutoDelete)
        {
            trans = trx;
            AcRuntimeInterop.DetachUnmanagedObject(dic);
            GC.SuppressFinalize(dic);
            m_includingErased = includingErased;
        }

        /// <summary>
        /// Gets a value indicating whether [including erased].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [including erased]; otherwise, <c>false</c>.
        /// </value>
        public new bool IncludingErased { get { return m_includingErased; } }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<T> GetEnumerator()
        {
            using (DbDictionaryEnumerator enumerator = base.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    do
                    {
                        yield return (T)trans.GetObject(enumerator.Current.Value, OpenMode.ForRead, m_includingErased, false);
                    } while (enumerator.MoveNext());
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}