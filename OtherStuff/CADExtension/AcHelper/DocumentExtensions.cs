using AcHelper.Wrappers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcHelper
{
    public static class DocumentExtensions
    {
        private const string DEFAULTCOMMAND = "Acad_Transaction";

        /// <summary>
        /// Starts a transaction for this document
        /// and executes an action with AcTransaction as parameter.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="action">Action to execute.</param>
        public static void StartTransaction(this Document doc, Action<AcTransaction> action)
        {
            doc.StartTransaction(DEFAULTCOMMAND, action);
        }
        /// <summary>
        /// Starts a transaction for this document
        /// and executes an action with AcTransaction as parameter.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="command">Command name under which the action is executed.</param>
        /// <param name="action">Action to execute.</param>
        public static void StartTransaction(this Document doc, string command, Action<AcTransaction> action)
        {
            command = command ?? DEFAULTCOMMAND;

            try
            {
                using (doc.LockDocument(DocumentLockMode.Write, command, command, true))
                {
                    using (AcTransaction tr = new AcTransaction(doc))
                    {
                        action(tr);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Starts a transaction for this document
        /// and executes an action with AcTransaction and ModelSpace as parameters.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="action">Action to execute.</param>
        public static void StartTransaction(this Document doc, Action<AcTransaction, BlockTableRecord> action)
        {
            doc.StartTransaction(DEFAULTCOMMAND, action);
        }
        /// <summary>
        /// Starts a transaction for this document
        /// and executes an action with AcTransaction and ModelSpace as parameters.
        /// The document will be locked before the transaction starts.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="command">Command name under which the action is executed.</param>
        /// <param name="action">Action to execute.</param>
        public static void StartTransaction(this Document doc, string command, Action<AcTransaction, BlockTableRecord> action)
        {
            // Just in case the command is null or empty.
            command = command ?? DEFAULTCOMMAND;
            try
            {
                // lock document for write
                using (doc.LockDocument(DocumentLockMode.Write, command, command, true))
                {
                    doc.TransactionManager.EnableGraphicsFlush(true);
                    using (AcTransaction tr = new AcTransaction(doc))
                    {
                        // Get modelspace from AcTransaction
                        // And make it write enabled.
                        var modelspace = tr.ModelSpace;
                        using (new WriteEnabler(doc, modelspace))
                        {
                            if (modelspace.IsWriteEnabled)
                            {
                                // Execute.
                                action(tr, modelspace);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public static void StartNestedTransaction(this Document doc, Action<NestableAcTransaction> action)
        {
            doc.StartNestedTransaction(DEFAULTCOMMAND, action);
        }
        public static void StartNestedTransaction(this Document doc, string command, Action<NestableAcTransaction> action)
        {
            command = command ?? DEFAULTCOMMAND;
            using (NestableAcTransaction tr = new NestableAcTransaction(doc))
            {
                action(tr);
            }
        }
        public static void StartNestedTransaction(this Document doc, Action<NestableAcTransaction, BlockTableRecord> action)
        {
            doc.StartNestedTransaction(DEFAULTCOMMAND, action);
        }
        public static void StartNestedTransaction(this Document doc, String command, Action<NestableAcTransaction, BlockTableRecord> action)
        {
            command = command ?? DEFAULTCOMMAND;
            doc.TransactionManager.EnableGraphicsFlush(true);
            try
            {

                using (NestableAcTransaction tr = new NestableAcTransaction(doc))
                {
                    var modelspace = tr.ModelSpace;
                    using (new WriteEnabler(doc, modelspace))
                    {
                        if (modelspace.IsWriteEnabled)
                        {
                            action(tr, modelspace);
                        }
                        else
                        {
                            throw new WriteEnablerException("Couldn't WriteEnable modelspace.");
                        }
                    }
                }
            }
            catch (WriteEnablerException weEx)
            {
                throw new AcTransactionException("Couldn't start a nested transaction.", weEx);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Adds an Entity to the modelspace of this document.
        /// Using a NestableTransaction so it commits directly after adding the entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="doc">Document</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ObjectId AddEntity<T>(this Document doc, Entity obj) where T : Entity
        {
            ObjectId id = ObjectId.Null;

            var ent = (T)obj;

            doc.StartNestedTransaction((tr, ms) =>
            {
                Transaction t = tr.Transaction;
                ms.AppendEntity(ent);
                t.AddNewlyCreatedDBObject(ent, true);
            });

            return id;
        }
    }
}
