using System.Collections.Generic;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// 
    /// </summary>
    public static class BlockTableExtensions
    {
        ///<overloads>
        /// <summmary>
        /// You can not edit a dependent block(a block that is inside from xref) from the referenced drawing. 
        /// The default is not to include dependent blocks since typically you will work on blocks that are contained 
        /// inside the drawing, and it saves time by not opening the DbObject and comparing 
        /// BlockTableRecord.ObjectId.OriginalDatabase.UnmanagedObject to the BlockTable.Database.UnmanagedObject property.
        /// </summmary>
        /// </overloads>
        /// <summary>
        /// Gets the block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter"><see cref="SymbolTableRecordFilter"/>.</param>
        /// <returns>IEnumerable{BlockTableRecord}</returns>

        public static IEnumerable<BlockTableRecord> GetBlockTableRecords(this BlockTable symbolTbl, Transaction trx,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            if (filter.IsSet(SymbolTableRecordFilter.IncludeDependent))
            {
                return symbolTbl.GetSymbolTableRecords<BlockTableRecord>(trx, mode, filter, true);
            }
            return symbolTbl.GetSymbolTableRecords<BlockTableRecord>(trx, mode, filter, true).NonDependent();
        }

        /// <summary>
        /// Gets the block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">OpenMode</param>
        /// <param name="filter"><see cref="SymbolTableRecordFilter"/>.</param>
        /// <returns>IEnumerable{BlockTableRecord}</returns>
        /// <example>
        /// <code source=".\Content\Samples\Samplescsharp\AcDbMgdExtensions\DatabaseServices\BlockTableExtensionsCommands.cs" language="cs" region="GetBlockTableRecords" />
        /// <code source=".\Content\Samples\Samplesvb\AcDbMgdExtensions\DatabaseServices\BlockTableExtensionsCommands.vb" language="VB" region="GetBlockTableRecords" />
        /// </example>
        public static IEnumerable<BlockTableRecord> GetBlockTableRecords(this BlockTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetBlockTableRecords(symbolTbl.Database.TransactionManager.TopTransaction, mode, filter);
        }


        /// <summary>
        /// Gets the user defined block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> GetUserDefinedBlockTableRecords(this BlockTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead)
        {
            return
                symbolTbl.GetSymbolTableRecords<BlockTableRecord>(trx, mode, SymbolTableRecordFilter.None, true)
                    .UserDefinedBlocks();
        }

        /// <summary>
        /// Gets the user defined block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> GetUserDefinedBlockTableRecords(this BlockTable symbolTbl,
            OpenMode mode = OpenMode.ForRead)
        {
            return symbolTbl.GetUserDefinedBlockTableRecords(symbolTbl.Database.TransactionManager.TopTransaction, mode);
        }


        /// <summary>
        /// Gets the user created block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> GetUserCreatedBlockTableRecords(this BlockTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead)
        {
            return
                symbolTbl.GetSymbolTableRecords<BlockTableRecord>(trx, mode, SymbolTableRecordFilter.None, true)
                    .UserCreatedBlocks();
        }

        /// <summary>
        /// Gets the user created block table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <returns></returns>
        public static IEnumerable<BlockTableRecord> GetUserCreatedBlockTableRecords(this BlockTable symbolTbl,
            OpenMode mode = OpenMode.ForRead)
        {
            return symbolTbl.GetUserCreatedBlockTableRecords(symbolTbl.Database.TransactionManager.TopTransaction, mode);
        }


        /// <summary>
        /// Gets the database block table identifier.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        internal static ObjectId GetDatabaseBlockTableId(this BlockTable symbolTbl, Database db)
        {
            return db.BlockTableId;
        }
    }
}