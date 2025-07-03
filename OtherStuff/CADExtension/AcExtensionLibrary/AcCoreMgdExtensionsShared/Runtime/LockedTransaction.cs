using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace Autodesk.AutoCAD.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public class LockedTransaction : Transaction
    {
        /// <summary>
        /// The document lock
        /// </summary>
        private DocumentLock docLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockedTransaction"/> class.
        /// </summary>
        /// <param name="trx">The TRX.</param>
        /// <param name="docLock">The document lock.</param>
        public LockedTransaction(Transaction trx, DocumentLock docLock)
           : base(trx.UnmanagedObject, trx.AutoDelete)
        {
            Interop.DetachUnmanagedObject(trx);
            GC.SuppressFinalize(trx);
            this.docLock = docLock;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="A_1"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool A_1)
        {
            base.Dispose(A_1);
            if (A_1)
            {
                docLock.Dispose();
            }
        }
    }
}