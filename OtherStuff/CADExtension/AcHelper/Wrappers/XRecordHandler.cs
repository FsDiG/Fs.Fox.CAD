using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace AcHelper.Wrappers
{
    using Collections;

    /// <summary>
    /// 
    /// </summary>
    public class XRecordHandler : IDisposable
    {
        #region Fields ...
        private Document _document = null;
        private Database _database = null;
        private ObjectId _nodId = ObjectId.Null;
        #endregion

        #region Ctor ...
        /// <summary>
        /// Initializes the Xrecord handler assosiated with Active Document.
        /// </summary>
        public XRecordHandler()
        {
            _document = Active.Document ?? throw new XRecordHandlerException("There is no Active Document present");
            _database = Active.Database;
            _nodId = _database.NamedObjectsDictionaryId;
        }
        #endregion

        #region Properties ...
        [Obsolete("Use the method from AcHelper.Collections.Dictionary class.", true)]
        /// <summary>
        /// Gets a Named Objects Dictionary with the given key
        /// If the collection does not contain the key, it will create a new Named Objects Dictionary instance.
        /// </summary>
        /// <param name="key">Name of the Named objects Dictionary</param>
        /// <returns>DBDictionary</returns>
        public DBDictionary GetNamedObjectsDictionary(string key)
        {
            DBDictionary nod = null;                // Named Objects Dictionary
            DBDictionary nod_collection = null;     // Named Objects Dictionary Collection

            try
            {
                _document.StartTransaction(tr =>
                {
                    Transaction t = tr.Transaction;
                    nod_collection = t.GetObject<DBDictionary>(_nodId, OpenMode.ForRead);

                    if (nod_collection.Contains(key))
                    {
                        ObjectId oid = nod_collection.GetAt(key);
                        nod = t.GetObject<DBDictionary>(oid, OpenMode.ForRead);
                    }
                    else
                    {
                        using (WriteEnabler we = new WriteEnabler(nod_collection))
                        {
                            if (nod_collection.IsWriteEnabled)
                            {
                                nod = new DBDictionary();
                                nod_collection.SetAt(key, nod);
                                t.AddNewlyCreatedDBObject(nod, true);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                string err_message = string.Format("DBDictionary named: '{0}' could not be found.", key);
                throw new XRecordHandlerException(err_message, ex);
            }

            return nod;
        }
        #endregion

        #region Get ...
        /// <summary>
        /// Gets the data from an Xrecord.
        /// </summary>
        /// <param name="dictionaryName">Name of Named Objects Dictionary</param>
        /// <param name="xKey">Key of Xrecord</param>
        /// <returns>ResultBuffer with data or Null if nothing found.</returns>
        /// <exception cref="Wrappers.XRecordException"/>
        public ResultBuffer GetXrecord(string dictionaryName, string xKey)
        {
            ResultBuffer result = null;

            try
            {
                if (_document != null)
                {
                    _document.StartTransaction(tr =>
                    {
                        Transaction t = tr.Transaction;
                        using (DBDictionary nod = Dictionaries.GetNamedObjectsDictionary(dictionaryName))
                        {
                            if (nod != null && nod.Contains(xKey))
                            {
                                ObjectId oid = nod.GetAt(xKey);
                                using (Xrecord xrec = t.GetObject<Xrecord>(oid, OpenMode.ForRead))
                                {
                                    result = xrec.Data;
                                }
                            }
                        }
                    });
                }
            }
            catch (XRecordHandlerException) { throw; }
            catch (Exception ex)
            {
                string err_message = string.Format("Unexpected error occured while retrieving xRecord '{0}' from Named Objects Dictionary '{1}'.", xKey, dictionaryName);
                throw new XRecordHandlerException(dictionaryName, xKey, err_message, ex, ErrorCode.XrecordNotFound);
            }

            return result;
        }
        /// <summary>
        /// Gets the data from an Entity Xrecord.
        /// </summary>
        /// <param name="entityId">ObjectId of the entity</param>
        /// <param name="xKey"></param>
        /// <returns></returns>
        public ResultBuffer GetEntityXrecord(ObjectId entityId, string xKey)
        {
            ResultBuffer result = null;

            if (_document != null)
            {
                try
                {
                    Common.UsingTransaction(_document, tr =>
                    {
                        Transaction t = tr.Transaction;
                        Entity ent = t.GetObject<Entity>(entityId, OpenMode.ForRead);
                        if (ent != null)
                        {
                            using (DBDictionary nod = t.GetObject<DBDictionary>(ent.ExtensionDictionary, OpenMode.ForRead))
                            {
                                if (nod != null && nod.Contains(xKey))
                                {
                                    ObjectId oid = nod.GetAt(xKey);
                                    using (Xrecord xrec = t.GetObject<Xrecord>(oid, OpenMode.ForRead))
                                    {
                                        result = xrec.Data;
                                    }
                                }
                                else if (!nod.Contains(xKey))
                                {
                                    string err_message = string.Format("Xrecord '{0}' not found in Entity Named Objects Dictionary", xKey);
                                    throw new XRecordHandlerException(err_message, ErrorCode.XrecordNotFound);
                                }
                            }
                        }
                        else
                        {
                            string err_message = string.Format("Object is not an entity");
                            throw new XRecordHandlerException(err_message, ErrorCode.NotAnEntity);
                        }

                    });
                }
                catch (System.Exception ex)
                {
                    string err_message = string.Format("Unexpected error occured while retrieving xRecord '{0}' from Entity Named Objects Dictionary.", xKey);
                    throw new XRecordHandlerException(err_message, ex);
                }
            }
            return result;
        }
        #endregion

        #region Update ...
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionaryName"></param>
        /// <param name="xKey"></param>
        /// <param name="resbuf"></param>
        /// <returns></returns>
        public bool UpdateDocumentXrecord(string dictionaryName, string xKey, ResultBuffer resbuf)
        {
            bool result = false;
            string err_message = string.Empty;

            try
            {
                _document?.StartTransaction(tr =>
                {
                    Transaction t = tr.Transaction;
                    using (DBDictionary nod = Dictionaries.GetNamedObjectsDictionary(dictionaryName))
                    {
                        result = UpdateXrecord(t, nod, xKey, resbuf);
                    }
                });

                //if (_document != null)
                //{
                //    Common.UsingTransaction(_document, tr =>
                //    {
                //        Transaction t = tr.Transaction;
                //        using (DBDictionary nod = Dictionaries.GetNamedObjectsDictionary(dictionaryName))
                //        {
                //            result = UpdateXrecord(t, nod, xKey, resbuf);
                //        }
                //    });
                //}
            }
            catch (XRecordHandlerException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                err_message = string.Format("An unexpected error occured while updating Xrecord '{0}' in Named Objects Dictionary '{1}'.", xKey, dictionaryName);
                throw new XRecordHandlerException(dictionaryName, xKey, err_message, ex);
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="xKey"></param>
        /// <param name="resbuf"></param>
        /// <returns></returns>
        public bool UpdateEntityXrecord(ObjectId entityId, string xKey, ResultBuffer resbuf)
        {
            string errMessage = string.Empty;
            bool result = false;
            try
            {
                Common.UsingTransaction(_document, tr =>
                {
                    Transaction t = tr.Transaction;
                    using (Entity ent = t.GetObject<Entity>(entityId, OpenMode.ForRead))
                    {
                        if (ent != null && !ent.IsErased)
                        {
                            using (WriteEnabler we_ent = new WriteEnabler(_document, ent))
                            {
                                if (ent.IsWriteEnabled)
                                {
                                    if (ent.ExtensionDictionary == null)
                                    {
                                        ent.CreateExtensionDictionary();
                                    }
                                    DBDictionary nod = t.GetObject<DBDictionary>(ent.ExtensionDictionary, OpenMode.ForRead);
                                    result = UpdateXrecord(t, nod, xKey, resbuf);
                                }
                                else
                                {
                                    throw new XRecordHandlerException("Couldn't open entity for write.");
                                }
                            }
                        }
                        else
                        {
                            errMessage = string.Format("Object is not an entity");
                            throw new XRecordHandlerException(errMessage, ErrorCode.NotAnEntity);
                        }
                    }
                });
                return result;
            }
            catch (XRecordHandlerException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                errMessage = string.Format("An unexpected error occured while updating Xrecord '{1}'.", xKey);
                throw new XRecordHandlerException(errMessage, ex);
            }
        }

        private bool UpdateXrecord(Transaction t, DBDictionary nod, string xKey, ResultBuffer resbuf)
        {
            string errMessage = string.Empty;

            using (new WriteEnabler(_document, nod))
            {
                if (nod.IsWriteEnabled)
                {
                    if (nod.Contains(xKey))
                    {
                        ObjectId oid = nod.GetAt(xKey);
                        using (Xrecord xrec = t.GetObject<Xrecord>(oid, OpenMode.ForRead))
                        {
                            if (xrec != null)
                            {
                                using (WriteEnabler we_xrec = new WriteEnabler(_document, xrec))
                                {
                                    if (xrec.IsWriteEnabled)
                                    {
                                        xrec.Data = resbuf;
                                        return true;
                                    }
                                    errMessage = string.Format("Could not open Xrecord '{0}' for write.", xKey);
                                }
                            }
                            errMessage = string.Format("Something went wrong with opening Xrecord '{0}'.", xKey);
                        }
                    }
                    else
                    {
                        using (Xrecord xrec = new Xrecord())
                        {
                            if (xrec != null)
                            {
                                xrec.Data = resbuf;
                                nod.SetAt(xKey, xrec);
                                t.AddNewlyCreatedDBObject(xrec, true);
                                return true;
                            }
                            errMessage = string.Format("Could not open Xrecord '{0}' for write.", xKey);
                        }
                    }
                }
                else
                {
                    errMessage = string.Format("Could not open Named Objects Dictionary for write to update Xrecord '{1}'.", xKey);
                    throw new XRecordHandlerException(errMessage, ErrorCode.NodNotFound);
                }
            }
            if (!string.IsNullOrEmpty(errMessage))
            {
                throw new XRecordHandlerException(errMessage);
            }
            return false;
        }
        #endregion

        #region Remove ...
        /// <summary>
        /// Removes Xrecord with given key
        /// </summary>
        /// <param name="dictionaryName">Name of Objects Dictionary.</param>
        /// <param name="xKey">Key of Xrecord to remove.</param>
        /// <returns>True if succeed, false if Xrecord does not exist.</returns>
        public bool RemoveDocumentXrecord(string dictionaryName, string xKey)
        {
            bool result = false;
            string err_message = string.Empty;

            try
            {
                Common.UsingTransaction(_document, tr =>
                {
                    Transaction t = tr.Transaction;
                    using (DBDictionary nod = Dictionaries.GetNamedObjectsDictionary(dictionaryName))
                    {
                        result = RemoveXrecord(nod, dictionaryName, xKey);
                    }
                });
                return result;
            }
            catch (WriteEnablerException)
            {
                throw new XRecordHandlerException(dictionaryName, xKey, "WriteEnabler countered an exception.", ErrorCode.Error);
            }
            catch (Exception ex)
            {
                err_message = string.Format("An unexpected error occured while removing Xrecord '{0}' from Named Objects Dictionary '{1}'", xKey, dictionaryName);
                throw new XRecordHandlerException(dictionaryName, xKey, err_message, ex);
            }
        }
        /// <summary>
        /// Removes Xrecord from Entity with given key.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="xKey">Key of Xrecord to remove.</param>
        /// <returns>True if succeed, False if Xrecord does not exist.</returns>
        public bool RemoveEntityXrecord(ObjectId entityId, string xKey)
        {
            bool result = false;
            string err_message = string.Empty;

            try
            {
                _document.StartTransaction(tr => 
                {
                    Transaction t = tr.Transaction;
                    using (var ent = t.GetObject<Entity>(entityId, OpenMode.ForRead))
                    {
                        if (ent != null)
                        {
                            using (new WriteEnabler(_document, ent))
                            {
                                if (ent.IsWriteEnabled)
                                {
                                    using (var nod = t.GetObject<DBDictionary>(ent.ExtensionDictionary, OpenMode.ForRead))
                                    {
                                        result = RemoveXrecord(nod, xKey);
                                    }
                                }
                                else
                                {
                                    throw new XRecordHandlerException("Couldn't open entity for write.");
                                }
                            }
                        }
                        else
                        {
                            err_message = string.Format("Object is not an entity");
                            throw new XRecordHandlerException(err_message, ErrorCode.NotAnEntity);
                        }
                    }
                });
                return result;
            }
            catch (WriteEnablerException)
            {
                throw new XRecordHandlerException(entityId.ToString(), xKey, "WriteEnabler countered an exception.", ErrorCode.Error);
            }
            catch (Exception ex)
            {
                err_message = string.Format("An unexpected error occured while removing Xrecord '{0}' from Named Objects Dictionary", xKey);
                throw new XRecordHandlerException(err_message, ex);
            }
        }
        /// <summary>
        /// Removes Xrecord with given key.
        /// </summary>
        /// <param name="nod">Named Object Dictionary containing the Xrecord.</param>
        /// <param name="xKey">Key of the Xrecord.</param>
        /// <param name="dictionaryName">If not Xrecord of Entity, the Named Object Dictionary has a name.</param>
        /// <returns>True if succeed, False if Xrecord does not exist.</returns>
        private bool RemoveXrecord(DBDictionary nod, string xKey, string dictionaryName = "")
        {
            string err_message;
            dictionaryName = dictionaryName == "" ? "\b" : dictionaryName; // Entity Nod has no name.

            if (nod != null)
            {
                if (!nod.Contains(xKey))
                {
                    return false;
                }

                using (new WriteEnabler(_document, nod))
                {
                    if (nod.IsWriteEnabled)
                    {
                        nod.Remove(xKey);
                        return true;
                    }
                    else
                    {
                        err_message = string.Format("Could not open Named Objects Dictionary '{0}' for write", dictionaryName);
                        throw new XRecordHandlerException(dictionaryName, xKey, err_message);
                    }
                }
            }
            else
            {
                err_message = string.Format("Could not get Named Objects Dictionary '{0}'", dictionaryName);
                throw new XRecordHandlerException(dictionaryName, xKey, err_message);
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                _document = null;
                _database = null;
                _nodId = ObjectId.Null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
