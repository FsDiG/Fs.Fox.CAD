namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension class for DBObject object
    /// </summary>
    public static class DBObjectExtensions
    {
        /// <summary>
        /// Determines whether [has extension dictionary].
        /// </summary>
        /// <param name="dbObj">The database object.</param>
        /// <returns>true if has a extension dictionary</returns>
        public static bool HasExtensionDictionary(this DBObject dbObj)
        {
            return !dbObj.ExtensionDictionary.IsNull;
        }

        /// <summary>
        /// Gets the extension dictionary.
        /// </summary>
        /// <param name="dbObj">The database object.</param>
        /// <param name="openMode">The open mode.</param>
        /// <returns>
        /// The existing extension dictionary or creates and add one if needed.
        /// use <see cref="HasExtensionDictionary" /> if only checking to see if it exist is needed
        /// </returns>
        public static DBDictionary GetExtensionDictionary(this DBObject dbObj, OpenMode openMode = OpenMode.ForRead)
        {
            if (!HasExtensionDictionary(dbObj))
            {
                if (!dbObj.IsWriteEnabled)
                {
                    dbObj.UpgradeOpen();
                }
                dbObj.CreateExtensionDictionary();
            }
            return dbObj.ExtensionDictionary.GetDBObject<DBDictionary>(openMode);
        }

        /// <summary>
        /// Tries the get extension dictionary ObjectId.
        /// </summary>
        /// <param name="dbObj">The database object.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static bool TryGetExtensionDictionaryId(this DBObject dbObj, out ObjectId id)
        {
            id = dbObj.ExtensionDictionary;
            return !id.IsNull;
        }

        /// <summary>
        /// Gets the extension dictionary identifier.
        /// </summary>
        /// <param name="dbObj">The database object.</param>
        /// <returns></returns>
        public static ObjectId GetExtensionDictionaryId(this DBObject dbObj)
        {
            ObjectId id;
            if (!dbObj.TryGetExtensionDictionaryId(out id))
            {
                if (!dbObj.IsWriteEnabled)
                {
                    dbObj.UpgradeOpen();
                }

                dbObj.CreateExtensionDictionary();
                id = dbObj.ExtensionDictionary;
            }
            return id;
        }

        /// <summary>
        /// Extensions the dictionary contains.
        /// </summary>
        /// <param name="dbObj">The database object.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static bool ExtensionDictionaryContains(this DBObject dbObj, string name)
        {
            if (dbObj.HasExtensionDictionary())
            {
                return dbObj.GetExtensionDictionary().Contains(name);
            }
            return false;
        }
    }
}