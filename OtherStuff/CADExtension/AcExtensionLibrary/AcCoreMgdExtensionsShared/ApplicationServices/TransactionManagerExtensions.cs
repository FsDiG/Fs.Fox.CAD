using Autodesk.AutoCAD.ApplicationServices;

namespace Autodesk.AutoCAD.ApplicationServices
{
    /// <summary>
    ///
    /// </summary>
    public static class TransactionManagerExtensions
    {
        /// <summary>
        /// Stars the locked transaction.
        /// </summary>
        /// <param name="tm">The tm.</param>
        /// <returns></returns>
        public static Runtime.LockedTransaction StartLockedTransaction(this TransactionManager tm)
        {
            DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument();
            return new Runtime.LockedTransaction(tm.StartTransaction(), doclock);
        }

        /// <summary>
        /// Stars the locked transaction.
        /// </summary>
        /// <param name="tm">The tm.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <param name="globalCommandName">Name of the global command.</param>
        /// <param name="localCommandName">Name of the local command.</param>
        /// <param name="promptIfFails">if set to <c>true</c> [prompt if fails].</param>
        /// <returns></returns>
        public static Runtime.LockedTransaction StartLockedTransaction(this TransactionManager tm, DocumentLockMode lockMode, string globalCommandName, string localCommandName, bool promptIfFails)
        {
            DocumentLock doclock = Application.DocumentManager.MdiActiveDocument.LockDocument(lockMode, globalCommandName, localCommandName, promptIfFails);
            return new Runtime.LockedTransaction(tm.StartTransaction(), doclock);
        }
    }
}