using System.Collections.Generic;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class LayerTableExtensions
    {
        /// <summary>
        /// Gets the layer table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="trx">The TRX.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<LayerTableRecord> GetLayerTableRecords(this LayerTable symbolTbl, Transaction trx,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetSymbolTableRecords<LayerTableRecord>(trx, mode, filter, false);
        }

        /// <summary>
        /// Gets the layer table records.
        /// </summary>
        /// <param name="symbolTbl">The symbol table.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static IEnumerable<LayerTableRecord> GetLayerTableRecords(this LayerTable symbolTbl,
            OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<LayerTableRecord>(symbolTbl.Database.TransactionManager.TopTransaction,
                    mode, filter, false);
        }
    }
}