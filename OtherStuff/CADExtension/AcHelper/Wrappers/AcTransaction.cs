using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace AcHelper.Wrappers
{
    public class AcTransaction : IDisposable
    {
        #region Fields ...
        // Statics
        /// <summary>
        /// The current transaction.
        /// </summary>
        private static Transaction _transaction = null;
        private static Document _document = null;
        private static Database _db = null;

        // Non-statics
        /// <summary>
        /// Is the transaction started and does it need a commit or abort.
        /// </summary>
        private bool _started = false;
        private BlockTableRecord _model_space = null;
        #endregion

        #region Ctor ...
        /// <summary>
        /// Starts a transaction if needed
        /// </summary>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception"/>
        public AcTransaction()
            : this(Active.Document)
        { }
        /// <summary>
        /// Starts a transaction if needed
        /// </summary>
        /// <param name="doc"></param>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception"/>
        internal AcTransaction(Document doc)
        {
            try
            {
                // is a transaction already running
                if (_transaction == null)
                {
                    _document = doc;
                    if (_document != null)
                    {
                        // no current transaction running; create one
                        _db = _document.Database;
                        _transaction = _document.TransactionManager.StartOpenCloseTransaction();
                        _model_space = GetModelSpace();
                        _started = true;
                    }
                    else
                    {
                        throw new Autodesk.AutoCAD.Runtime.Exception(Autodesk.AutoCAD.Runtime.ErrorStatus.NoDocument, "No active document");
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception acEx)
            {
                throw new AcTransactionException("No Active document for a valid transaction", acEx);
            }
            catch (Exception ex)
            {
                throw new AcTransactionException("An unexpected error occured while starting a transaction.", ex);
            }
        }
        #endregion

        #region Properties ...
        /// <summary>
        /// <see cref="Database.Transaction"/>
        /// </summary>
        public Transaction Transaction
        {
            get { return _transaction; }
        }
        /// <summary>
        /// Gets the ModelSpace BlockTableRecord.
        /// </summary>
        public BlockTableRecord ModelSpace
        {
            get { return _model_space; }
        }
        /// <summary>
        /// Is the transaction started and does it need a commit or abort.
        /// </summary>
        public bool IsStarted
        {
            get { return _started; }
        }
        /// <summary>
        /// Document of the transaction.
        /// </summary>
        public Document Document
        {
            get { return _document; }
        }
        #endregion

        #region Methods ...
        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            _started = false;
        }
        /// <summary>
        /// Abort the current transaction
        /// </summary>
        public void Abort()
        {
            if (_transaction != null)
            {
                _transaction.Abort();
                _transaction.Dispose();
                _transaction = null;
            }
        }
        #endregion

        #region Helpers ...
        private BlockTableRecord GetModelSpace()
        {
            var block_table = _transaction.GetObject<BlockTable>(_db.BlockTableId, OpenMode.ForRead);
            var model_space = _transaction.GetObject<BlockTableRecord>(block_table[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            return model_space;
        }
        #endregion

        #region Dispose ...
        /// <summary>
        /// closes the transaction
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// disposes the object
        /// </summary>
        /// <param name="disposing">the disposing status input parameter</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_started)
                {
                    Commit();
                } //if
            } //if
        }
        #endregion
    }
}
