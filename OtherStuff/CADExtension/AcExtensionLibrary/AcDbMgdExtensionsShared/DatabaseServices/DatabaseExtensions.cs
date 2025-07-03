using Autodesk.AutoCAD.Runtime;
using System;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension class for Database object
    /// </summary>

    public static class DatabaseExtensions
    {
        /// <summary>
        /// The standard
        /// </summary>
        private static string standard = "Standard";

        /// <summary>
        /// Opens the BlockTable
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>BlockTable</returns>
        /// <exception cref="Exception"></exception>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="BlockTableTrx" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="BlockTableTrx" />
        /// </example>
        public static BlockTable BlockTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (BlockTable)transaction.GetObject(database.BlockTableId, openMode, false, false);
        }

        /// <summary>
        /// Opens the BlockTable
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>BlockTable</returns>
        /// <exception cref="Exception"></exception>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="BlockTable" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="BlockTable" />
        /// </example>
        public static BlockTable BlockTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.BlockTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Dims the style table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>DimStyleTable</returns>
        /// <exception cref="Exception"></exception>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="DimStyleTableTrx" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="DimStyleTableTrx" />
        /// </example>
        public static DimStyleTable DimStyleTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DimStyleTable)transaction.GetObject(database.DimStyleTableId, openMode, false, false);
        }

        /// <summary>
        /// Dims the style table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>DimStyleTable</returns>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="DimStyleTable" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="DimStyleTable" />
        /// </example>
        public static DimStyleTable DimStyleTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.DimStyleTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Layers the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>LayerTable</returns>
        /// <exception cref="Exception"></exception>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="LayerTableTrx" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="LayerTableTrx" />
        /// </example>
        public static LayerTable LayerTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (LayerTable)transaction.GetObject(database.LayerTableId, openMode, false, false);
        }

        /// <summary>
        /// Layers the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>LayerTable</returns>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.cs" language="cs" region="LayerTable" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\DatabaseExtensionsCommands.vb" language="VB" region="LayerTable" />
        public static LayerTable LayerTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.LayerTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Linetypes the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>LinetypeTable</returns>
        /// <exception cref="Exception"></exception>
        public static LinetypeTable LinetypeTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (LinetypeTable)transaction.GetObject(database.LinetypeTableId, openMode, false, false);
        }

        /// <summary>
        /// Linetypes the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>LinetypeTable</returns>
        public static LinetypeTable LinetypeTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.LinetypeTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Regs the application table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>RegAppTable</returns>
        /// <exception cref="Exception"></exception>
        public static RegAppTable RegAppTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (RegAppTable)transaction.GetObject(database.RegAppTableId, openMode, false, false);
        }

        /// <summary>
        /// Regs the application table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>RegAppTable</returns>
        public static RegAppTable RegAppTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.RegAppTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Texts the style table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>TextStyleTable</returns>
        /// <exception cref="Exception"></exception>
        public static TextStyleTable TextStyleTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (TextStyleTable)transaction.GetObject(database.TextStyleTableId, openMode, false, false);
        }

        /// <summary>
        /// Texts the style table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>TextStyleTable</returns>
        public static TextStyleTable TextStyleTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.TextStyleTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Standards the text style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public static ObjectId StandardTextStyle(this Database database)
        {
            return SymbolUtilityServices.GetTextStyleStandardId(database);
        }



        public static ObjectId StandardTableStyle(this Database database)
        {
            return database.TableStyleDBDictionary().GetAt(standard);
        }


        /// <summary>
        /// Continuouses the linetype identifier.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public static ObjectId ContinuousLinetypeId(this Database database)
        {
            return SymbolUtilityServices.GetLinetypeContinuousId(database);
        }

        /// <summary>
        /// Standards the dim style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public static ObjectId StandardDimStyle(this Database database, Transaction transaction)
        {
            var dimtbl = database.DimStyleTable(transaction);
            return dimtbl.Has(standard) ? dimtbl[standard] : database.Dimstyle;
        }

        /// <summary>
        /// Standards the dim style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public static ObjectId StandardDimStyle(this Database database)
        {
            return database.StandardDimStyle(database.TransactionManager.TopTransaction);
        }

        /// <summary>
        /// Ucses the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>UcsTable</returns>
        /// <exception cref="Exception"></exception>
        public static UcsTable UcsTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (UcsTable)transaction.GetObject(database.UcsTableId, openMode, false, false);
        }

        /// <summary>
        /// Ucses the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>UcsTable</returns>
        public static UcsTable UcsTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.UcsTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Viewports the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>ViewportTable</returns>
        /// <exception cref="Exception"></exception>
        public static ViewportTable ViewportTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (ViewportTable)transaction.GetObject(database.ViewportTableId, openMode, false, false);
        }

        /// <summary>
        /// Viewports the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>ViewportTable</returns>
        public static ViewportTable ViewportTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.ViewportTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Views the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>ViewTable</returns>
        /// <exception cref="Exception"></exception>
        public static ViewTable ViewTable(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (ViewTable)transaction.GetObject(database.ViewTableId, openMode, false, false);
        }

        /// <summary>
        /// Views the table.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns></returns>
        public static ViewTable ViewTable(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.ViewTable(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Author: Tony Tanzillo
        /// Source: http://www.theswamp.org/index.php?topic=41311.msg464457#msg464457
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the BlockTableRecord for "ModelSpace"</returns>
        /// <exception cref="Exception"></exception>
        public static BlockTableRecord ModelSpace(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (BlockTableRecord)transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(database), openMode, false, false);
        }

        /// <summary>
        /// Models the space.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the BlockTableRecord for "ModelSpace"</returns>
        public static BlockTableRecord ModelSpace(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.ModelSpace(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Author: Tony Tanzillo
        /// Source: http://www.theswamp.org/index.php?topic=41311.msg464457#msg464457
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the BlockTableRecord for the database's Current Space</returns>
        /// <exception cref="Exception"></exception>
        public static BlockTableRecord CurrentSpace(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, openMode, false, false);
        }

        /// <summary>
        /// Currents the space.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the BlockTableRecord for the database's Current Space</returns>
        public static BlockTableRecord CurrentSpace(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.CurrentSpace(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Nameds the object database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary that is the Named Object Dictionary</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary NamedObjectDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.NamedObjectsDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Nameds the object database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary that is the Named Object Dictionary</returns>
        public static DBDictionary NamedObjectDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.NamedObjectDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Groups the database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for Groups</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary GroupDBDictionary(this Database database, Transaction transaction, OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.GroupDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Groups the database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for Groups</returns>
        public static DBDictionary GroupDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.GroupDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Mls the style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for MLStyleDBDictionary</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary MLStyleDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.MLStyleDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Mls the style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for MLStyleDBDictionary</returns>
        public static DBDictionary MLStyleDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.MLStyleDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Standards the ml style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public static ObjectId StandardMLStyle(this Database database)
        {
            return database.StandardMLStyle(database.TransactionManager.TopTransaction);
        }

        /// <summary>
        /// Standards the ml style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public static ObjectId StandardMLStyle(this Database database, Transaction transaction)
        {
            var mlDic = database.MLStyleDBDictionary(transaction);
            return mlDic.Contains(standard) ? mlDic.GetAt(standard) : database.CmlstyleID;
        }

        /// <summary>
        /// Layouts the database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for Layouts</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary LayoutDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.LayoutDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Layouts the database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for Layouts</returns>
        public static DBDictionary LayoutDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.LayoutDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Plots the settings database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for PlotSettings</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary PlotSettingsDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.PlotSettingsDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Plots the settings database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for PlotSettings</returns>
        public static DBDictionary PlotSettingsDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.PlotSettingsDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Tables the style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for TableStyles</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary TableStyleDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.TableStyleDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// Tables the style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for TableStyles</returns>
        public static DBDictionary TableStyleDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.TableStyleDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// ms the leader style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for MLeaderStyles</returns>
        /// <exception cref="Exception"></exception>
        public static DBDictionary MLeaderStyleDBDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            return (DBDictionary)transaction.GetObject(database.MLeaderStyleDictionaryId, openMode, false, false);
        }

        /// <summary>
        /// ms the leader style database dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <returns>Returns the DbDictionary for MLeaderStyles</returns>
        public static DBDictionary MLeaderStyleDBDictionary(this Database database, OpenMode openMode = OpenMode.ForRead)
        {
            return database.MLeaderStyleDBDictionary(database.TransactionManager.TopTransaction, openMode);
        }

        /// <summary>
        /// Standards the m leader style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        public static ObjectId StandardMLeaderStyle(this Database database)
        {
            return database.StandardMLeaderStyle(database.TransactionManager.TopTransaction);
        }

        /// <summary>
        /// Standards the m leader style.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public static ObjectId StandardMLeaderStyle(this Database database, Transaction transaction)
        {
            var mlDic = database.MLeaderStyleDBDictionary(transaction);
            return mlDic.Contains(standard) ? mlDic.GetAt(standard) : database.MLeaderstyle;
        }

        /// <summary>
        /// Groups the dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static GroupDictionary GroupDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead, bool includingErased = false)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            DBDictionary dic = (DBDictionary)transaction.GetObject(database.GroupDictionaryId, openMode, false, false);
            return includingErased
                ? new GroupDictionary(transaction, dic, includingErased)
                : new GroupDictionary(transaction, dic.IncludingErased, includingErased);
        }

        /// <summary>
        /// Groups the dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        public static GroupDictionary GroupDictionary(this Database database, OpenMode openMode = OpenMode.ForRead,
            bool includingErased = false)
        {
            return database.GroupDictionary(database.TransactionManager.TopTransaction, openMode, includingErased);
        }

        /// <summary>
        /// Mls the style dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static MLStyleDictionary MLStyleDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead, bool includingErased = false)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            DBDictionary dic = (DBDictionary)transaction.GetObject(database.MLStyleDictionaryId, openMode, false, false);
            return includingErased
                ? new MLStyleDictionary(transaction, dic, includingErased)
                : new MLStyleDictionary(transaction, dic.IncludingErased, includingErased);
        }

        /// <summary>
        /// Mls the style dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        public static MLStyleDictionary MLStyleDictionary(this Database database, OpenMode openMode = OpenMode.ForRead,
            bool includingErased = false)
        {
            return database.MLStyleDictionary(database.TransactionManager.TopTransaction, openMode, includingErased);
        }

        /// <summary>
        /// Layouts the dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LayoutDictionary LayoutDictionary(this Database database, Transaction transaction,
            OpenMode openMode = OpenMode.ForRead, bool includingErased = false)
        {
            if (transaction == null)
            {
                throw new Exception(ErrorStatus.NoActiveTransactions);
            }
            DBDictionary dic = (DBDictionary)transaction.GetObject(database.LayoutDictionaryId, openMode, false, false);
            return includingErased
                ? new LayoutDictionary(transaction, dic, includingErased)
                : new LayoutDictionary(transaction, dic.IncludingErased, includingErased);
        }

        /// <summary>
        /// Layouts the dictionary.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="openMode">The openMode.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        /// <returns></returns>
        public static LayoutDictionary LayoutDictionary(this Database database, OpenMode openMode = OpenMode.ForRead,
            bool includingErased = false)
        {
            return database.LayoutDictionary(database.TransactionManager.TopTransaction, openMode, includingErased);
        }

        ///// <summary>
        ///// Revisions the number.
        ///// </summary>
        ///// <param name="database">The database.</param>
        ///// <returns></returns>
        //public static int RevisionNumber(this Database database)
        //{
        //    DatabaseSummaryInfo info = database.SummaryInfo;
        //    string revisionNumberString = info.RevisionNumber;
        //    int revisionNumber;
        //    if (!revisionNumberString.IsNullOrEmpty())
        //    {
        //        if (Int32.TryParse(revisionNumberString, out revisionNumber))
        //        {
        //            return revisionNumber;
        //        }
        //    }
        //    revisionNumber = 0;
        //    DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(info);
        //    infoBuilder.RevisionNumber = revisionNumber.ToString();
        //    database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        //    return revisionNumber;
        //}

        ///// <summary>
        ///// Increments the revision number.
        ///// </summary>
        ///// <param name="database">The database.</param>
        ///// <returns></returns>
        //public static int IncrementRevisionNumber(this Database database)
        //{
        //    return database.AddToRevisionNumber(1);
        //}

        ///// <summary>
        ///// Adds to revision number.
        ///// </summary>
        ///// <param name="database">The database.</param>
        ///// <param name="number">The number.</param>
        ///// <returns></returns>
        //public static int AddToRevisionNumber(this Database database, int number)
        //{
        //    DatabaseSummaryInfo info = database.SummaryInfo;
        //    string revisionNumberString = info.RevisionNumber;
        //    int revisionNumber;
        //    if (revisionNumberString.IsNullOrEmpty() || !Int32.TryParse(revisionNumberString, out revisionNumber))
        //    {
        //        revisionNumber = 0;
        //    }

        //    int newRevisionNum = revisionNumber + number;
        //    if (newRevisionNum < 0)
        //    {
        //        newRevisionNum = 0;
        //    }
        //    DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
        //    infoBuilder.RevisionNumber = newRevisionNum.ToString();
        //    database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        //    return newRevisionNum;
        //}
    }
}