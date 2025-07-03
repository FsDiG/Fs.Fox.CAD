using System.Collections.Generic;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension class for DimStyleTable object
    /// </summary>
    public static class DimStyleTableExtensions
    {
        /// <summary>
        /// Gets the dim style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<DimStyleTableRecord> GetDimStyleTableRecords(this DimStyleTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead,
            SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetSymbolTableRecords<DimStyleTableRecord>(trx, mode, filter, true);
        }

        /// <summary>
        /// Gets the dim style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<DimStyleTableRecord> GetDimStyleTableRecords(this DimStyleTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<DimStyleTableRecord>(
                    symbolTbl.Database.TransactionManager.TopTransaction, mode, filter, true);
        }
    }
}