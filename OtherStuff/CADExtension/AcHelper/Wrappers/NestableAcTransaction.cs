using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace AcHelper.Wrappers
{
    /// <summary>
    /// 
    /// </summary>
    public class NestableAcTransaction : IDisposable
    {
        private Document _document;
        private Transaction _transaction;
        BlockTableRecord _modelspace;

        /// <summary>
        /// 
        /// </summary>
        public NestableAcTransaction() : this(Active.Document)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        internal NestableAcTransaction(Document document)
        {
            try
            {
                if (document != null)
                {
                    _document = document;
                    _transaction = _document.TransactionManager.StartTransaction();
                    _modelspace = GetModelSpace();
                }
                else
                {
                    throw new Autodesk.AutoCAD.Runtime.Exception(Autodesk.AutoCAD.Runtime.ErrorStatus.NoDocument, "No active document");
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception acEx)
            {
                switch (acEx.ErrorStatus)
                {
                    case Autodesk.AutoCAD.Runtime.ErrorStatus.NoDocument:
                        throw new AcTransactionException("No Active document for a valid transaction", acEx);
                    default:
                        throw new AcTransactionException("AutoCAD ran into an exception while creating a transaction.", acEx);
                }
            }
            catch (Exception ex)
            {
                throw new AcTransactionException("An unexpected error occured while starting a transaction.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Transaction Transaction => _transaction;
        /// <summary>
        /// 
        /// </summary>
        public BlockTableRecord ModelSpace => _modelspace;

        /// <summary>
        /// 
        /// </summary>
        public void Commit()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Abort()
        {
            _transaction.Abort();
            _transaction.Dispose();
            _transaction = null;
        }

        private BlockTableRecord GetModelSpace()
        {
            var blocktable = _transaction.GetObject<BlockTable>(_document.Database.BlockTableId, OpenMode.ForRead);
            var modelspace = _transaction.GetObject<BlockTableRecord>(blocktable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            return modelspace;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// After commiting it disposes the NestableAcTransaction object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Commit();
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
