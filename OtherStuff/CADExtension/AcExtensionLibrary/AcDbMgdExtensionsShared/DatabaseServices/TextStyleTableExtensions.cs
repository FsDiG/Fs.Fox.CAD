using System.Collections.Generic;
using System.Linq;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class TextStyleTableExtensions
    {
        /// <summary>
        /// Gets all text style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetAllTextStyleTableRecords(this TextStyleTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead,
            SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetSymbolTableRecords<TextStyleTableRecord>(trx, mode, filter, true);
        }

        /// <summary>
        /// Gets all text style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetAllTextStyleTableRecords(this TextStyleTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<TextStyleTableRecord>(
                    symbolTbl.Database.TransactionManager.TopTransaction, mode, filter, true);
        }

        /// <summary>
        /// Gets the text style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetTextStyleTableRecords(this TextStyleTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead,
            SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<TextStyleTableRecord>(trx, mode, filter, true)
                    .Where(txt => !txt.IsShapeFile);
        }

        /// <summary>
        /// Gets the text style table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetTextStyleTableRecords(this TextStyleTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetTextStyleTableRecords(symbolTbl.Database.TransactionManager.TopTransaction, mode, filter);
        }

        /// <summary>
        /// Gets the shape file table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetShapeFileTableRecords(this TextStyleTable symbolTbl,
            Transaction trx, OpenMode mode = OpenMode.ForRead,
            SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<TextStyleTableRecord>(trx, mode, filter, true)
                    .Where(txt => txt.IsShapeFile);
        }

        /// <summary>
        /// Gets the shape file table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<TextStyleTableRecord> GetShapeFileTableRecords(this TextStyleTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetShapeFileTableRecords(symbolTbl.Database.TransactionManager.TopTransaction, mode, filter);
        }
    }
}