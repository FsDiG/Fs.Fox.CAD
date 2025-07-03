using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension class for DBDictionary object
    /// </summary>
    public static class DBDictionaryExtensions
    {
        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception"></exception>
        public static IEnumerable<T> GetEntries<T>(this DBDictionary dic, Transaction trx,
            OpenMode mode = OpenMode.ForRead, bool includingErased = false) where T : DBObject
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            foreach (var entry in includingErased ? dic.IncludingErased : dic)
            {
                yield return (T)trx.GetObject(entry.Value, mode, includingErased, false);
            }
        }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEntries<T>(this DBDictionary dic, OpenMode mode = OpenMode.ForRead,
            bool includingErased = false) where T : DBObject
        {
            return dic.GetEntries<T>(dic.Database.TransactionManager.TopTransaction, mode, includingErased);
        }

        public static IEnumerable<DBDictionaryEntry> GetEntries(this DBDictionary dic, Transaction trx,
    OpenMode mode = OpenMode.ForRead, bool includingErased = false) 
        {
            if (trx == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            foreach (var entry in includingErased ? dic.IncludingErased : dic)
            {
                yield return entry;
            }
        }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        public static IEnumerable<DBDictionaryEntry> GetEntries(this DBDictionary dic, OpenMode mode = OpenMode.ForRead,
            bool includingErased = false)
        {
            return dic.GetEntries(dic.Database.TransactionManager.TopTransaction, mode, includingErased);
        }

    }
}