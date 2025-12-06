using AcHelper.Wrappers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace AcHelper
{
    public static class Common
    {
        /// <summary>
        /// Wrapper for the active Database's TransactionManager.
        /// The wrapper locks the document before starting a transaction!
        /// </summary>
        /// <param name="action">Method which uses the Transaction.</param>
        /// <param name="commandName">Name of action the TransactionManager is executing.</param>
        public static void UsingTransaction(Action<AcTransaction> action, string commandName = "")
        {
            UsingTransaction(Active.Document, commandName, action);
        }
        /// <summary>
        /// Wrapper for the active Database's TransactionManager.
        /// The wrapper locks the document before starting a transaction!
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="action">Method which uses the Transaction.</param>
        public static void UsingTransaction(Document doc, Action<AcTransaction> action)
        {
            UsingTransaction(doc, string.Empty, action);
        }
        /// <summary>
        /// Wrapper for the active Database's TransactionManager.
        /// The wrapper locks the document before starting a transaction!
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="commandName">Name of action the TransactionManager is executing.</param>
        /// <param name="action">Method which uses the Transaction.</param>
        /// <exception cref="AcHelper.Exceptions.TransactionException"/>
        public static void UsingTransaction(Document doc, string commandName, Action<AcTransaction> action)
        {
            commandName = commandName ?? "Acad_Transaction"; // commandName == "" ? "Acad_Transaction" : commandName;

            try
            {
                using (DocumentLock doclock = doc.LockDocument(DocumentLockMode.Write, commandName, commandName, true))
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
        /// Wrapper for the active Document's ModelSpace.
        /// The wrapper locks the document before starting a transaction!
        /// </summary>
        /// <param name="action">Method which uses the Transaction.</param>
        /// <param name="commandName">Name of action the TransactionManager is executing.</param>
        public static void UsingModelSpace(Action<AcTransaction, BlockTableRecord> action, string commandName = "")
        {
            UsingModelSpace(Active.Document, commandName, action);
        }
        /// <summary>
        /// Wrapper for the active Document's ModelSpace.
        /// The wrapper locks the document before starting a transaction!
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="action">Method which uses the Transaction.</param>
        public static void UsingModelSpace(Document doc, Action<AcTransaction, BlockTableRecord> action)
        {
            UsingModelSpace(doc, "", action);
        }
        /// <summary>
        /// Wrapper for the active Document's ModelSpace.
        /// Makes the ModelSpace writable and
        /// locks the document before starting a transaction!
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="commandName">Name of action the TransactionManager is executing.</param>
        /// <param name="action"></param>
        /// <exception cref="AcHelper.Exceptions.TransactionException"/>
        public static void UsingModelSpace(Document doc, string commandName, Action<AcTransaction, BlockTableRecord> action)
        {
            commandName = commandName == "" ? "Acad_Transaction" : commandName;
            try
            {
                using (DocumentLock doclock = doc.LockDocument(DocumentLockMode.Write, commandName, commandName, true))
                {
                    doc.TransactionManager.EnableGraphicsFlush(true);
                    using (AcTransaction tr = new AcTransaction(doc))
                    {
                        var modelspace = tr.ModelSpace;     // Modelspace

                        using (WriteEnabler we = new WriteEnabler(doc, modelspace))
                        {
                            if (modelspace.IsWriteEnabled)
                            {
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
    }
}
