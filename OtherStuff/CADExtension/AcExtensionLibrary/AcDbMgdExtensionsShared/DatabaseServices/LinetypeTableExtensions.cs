using System.Collections.Generic;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class LinetypeTableExtensions
    {
        /// <summary>
        /// Gets the linetype table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<LinetypeTableRecord> GetLinetypeTableRecords(this LinetypeTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead,
            SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetSymbolTableRecords<LinetypeTableRecord>(trx, mode, filter, false);
        }

        /// <summary>
        /// Gets the linetype table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<LinetypeTableRecord> GetLinetypeTableRecords(this LinetypeTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<LinetypeTableRecord>(
                    symbolTbl.Database.TransactionManager.TopTransaction, mode, filter, false);
        }
    }
}