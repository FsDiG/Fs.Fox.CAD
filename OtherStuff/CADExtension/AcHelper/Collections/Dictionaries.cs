using AcHelper.Wrappers;
using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace AcHelper.Collections
{
    public static class Dictionaries
    {
        public const string ACAD_MLINESTYLE = "ACAD_MLINESTYLE";

        #region Dictionary
        /// <summary>
        /// Retreive Dictionary from database with specific type and name
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="dictionaryType"></param>
        /// <param name="dictionaryName"></param>
        /// <returns></returns>
        public static DBDictionary GetDictionary(Transaction transaction, string dictionaryType, string dictionaryName)
        {
            DBDictionary nod_collection = GetNodCollection(transaction);
            DBDictionary dictionary = transaction.GetObject<DBDictionary>(nod_collection.GetAt(dictionaryType), OpenMode.ForRead);
            foreach (DBDictionaryEntry entry in dictionary)
            {
                if (entry.Key == dictionaryName)
                {
                    return dictionary;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets a Named Objects Dictionary with the given key
        /// If the collection does not contain the key, it will create a new Named Objects Dictionary instance.
        /// </summary>
        /// <param name="name">Name of the Named Objects Dictionary.</param>
        /// <param name="create">True if the Dictionary must be created if not exist.</param>
        /// <returns>Named Objects Dictionary. New if <paramref name="create"/> is true and the Dictionary does not exist; Otherwise it will be null if the Dictionary</returns>
        public static DBDictionary GetNamedObjectsDictionary(string name, bool create)
        {
            DBDictionary nod = null;                // Named Objects Dictionary
            Database db = Active.Database;

            try
            {
                Active.Document.StartTransaction(tr =>
                {
                    Transaction t = tr.Transaction;
                    DBDictionary nod_collection = t.GetObject<DBDictionary>(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                    if (nod_collection.Contains(name))
                    {
                        ObjectId oid = nod_collection.GetAt(name);
                        nod = t.GetObject<DBDictionary>(oid, OpenMode.ForRead);
                    }
                    else if (create)
                    {
                        nod = AddNamedObjectsDictionary(t, name);
                    }
                });
            }
            catch (XRecordHandlerException) { throw; }
            catch (Exception ex)
            {
                string err_message = string.Format("DBDictionary named: '{0}' could not be found.", name);
                throw new XRecordHandlerException(err_message, ex);
            }

            return nod;
        }
        /// <summary>
        /// Gets a Named Objects Dictionary with the given <paramref name="name"/>.
        /// If the collection does not contain the key, it will NOT create a new Named Objects Dictionary instance.
        /// </summary>
        /// <param name="name">Name of the Named objects Dictionary</param>
        /// <returns>DBDictionary; Null if it does not exist.</returns>
        public static DBDictionary GetNamedObjectsDictionary(string name)
        {
            return GetNamedObjectsDictionary(name, false);
        }
        /// <summary>
        /// Gets the Named Objects Dictionary collection from the drawing.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static DBDictionary GetNodCollection(Transaction transaction)
        {
            Database db = Active.Database;
            DBDictionary nod_collection = transaction.GetObject<DBDictionary>(db.NamedObjectsDictionaryId, OpenMode.ForRead);
            return nod_collection;
        }
        /// <summary>
        /// Adds a new DBDictionary to the nod Collection of the drawing.
        /// </summary>
        /// <param name="transaction">Running transaction.</param>
        /// <param name="name">Name of the Dictionary to add to the collection.</param>
        public static DBDictionary AddNamedObjectsDictionary(Transaction transaction, string name)
        {
            Database db = Active.Database;
            DBDictionary newNod = null;
            DBDictionary nod_collection = GetNodCollection(transaction);

            if (!nod_collection.Contains(name))
            {
                using (new WriteEnabler(nod_collection))
                {
                    if (nod_collection.IsWriteEnabled)
                    {
                        newNod = new DBDictionary();
                        nod_collection.SetAt(name, newNod);
                        transaction.AddNewlyCreatedDBObject(newNod, true);
                    }
                    else
                    {
                        string err_message = string.Format("DBDictionary named: '{0}' could not be created.", name);
                        throw new XRecordHandlerException(err_message);
                    }
                }
            }
            return newNod;
        }
        /// <summary>
        /// Removes a DBDictionary with the given name from the drawing.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="name"></param>
        public static void RemoveNamedObjectsDictionary(Transaction transaction, string name)
        {
            Database db = Active.Database;
            DBDictionary nod_collection = GetNodCollection(transaction);

            if (nod_collection.Contains(name))
            {
                nod_collection.Remove(name);
            }
        }
        #endregion

        #region MLine dictionary
        /// <summary>
        /// Gets a MLineStyle from the ACAD_MLINESTYLE dictionary.
        /// </summary>
        /// <param name="transaction">Running transaction.</param>
        /// <param name="styleName">Name of the Style.</param>
        /// <returns>MLineStyle if the name exists; Otherwise null.</returns>
        public static MlineStyle GetMultilineStyle(Transaction transaction, string styleName)
        {
            MlineStyle style = null;
            DBDictionary mlineStyles = GetNamedObjectsDictionary(ACAD_MLINESTYLE);

            if (mlineStyles.Contains(styleName))
            {
                style = transaction.GetObject<MlineStyle>(mlineStyles.GetAt(styleName), OpenMode.ForRead);
            }
            return style;
        }
        /// <summary>
        /// Adding MultilineStyle with name and file
        /// </summary>
        /// <param name="multiLine">struct AcMultiline containing all properties.</param>
        public static void AddMultilineStyle(AcMultiLine multiLine)
        {
            Database database = Active.Database;

            //Load multilinestyle
            database.LoadMlineStyleFile(multiLine.Name, multiLine.FileName);
        }

        /// <summary>
        /// Removes a MLineStyle from the ACAD_MLINESTYLE dictionary.
        /// </summary>
        /// <param name="transaction">Running transaction.</param>
        /// <param name="styleName">Name of the MLineStyle.</param>
        public static void RemoveMultilineStyle(Transaction transaction, string styleName)
        {
            Database db = Active.Database;
            DBDictionary mlineStyles = GetNamedObjectsDictionary(ACAD_MLINESTYLE);
            if (mlineStyles.Contains(styleName))
            {
                mlineStyles.Remove(styleName);
            }
        }
        /// <summary>
        /// Removes a MLineStyle from the ACAD_MLINESTYLE dictionary.
        /// </summary>
        /// <param name="transaction">Running transaction.</param>
        /// <param name="styleId">ObjectId of the MLineStyle.</param>
        public static void RemoveMultilineStyle(Transaction transaction, ObjectId styleId)
        {
            Database db = Active.Database;
            DBDictionary mlineStyles = GetNamedObjectsDictionary(ACAD_MLINESTYLE);
            if (mlineStyles.Contains(styleId))
            {
                mlineStyles.Remove(styleId);
            }
        }
        /// <summary>
        /// This method will be removed either way.
        /// </summary>
        /// <param name="dictionaryType"></param>
        /// <param name="dictionaryName"></param>
        [Obsolete("Use the RemoveMultiLineStyle(Transaction transaction, string styleName) method instead. This method will be removed the next version.")]
        public static void RemoveMultilineStyle(string dictionaryType, string dictionaryName)
        {
            Database db = Active.Database;

            try
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    DBDictionary nod = GetNodCollection(transaction);
                    DBDictionary dict = transaction.GetObject<DBDictionary>(nod.GetAt(dictionaryType), OpenMode.ForWrite);
                    foreach (DBDictionaryEntry entry in dict)
                    {
                        if (entry.Key == dictionaryName)
                        {
                            dict.Remove(entry.Key);
                        }
                    }

                    transaction.Commit();
                }//using
            }
            catch (Exception ex)
            {
                Active.WriteMessage(ex.Message);
                throw;
            }
        }
        #endregion
    }
}
