using System;
using System.Collections.Generic;
using System.Text;

namespace Autodesk.AutoCAD.DatabaseServices
{
    public static class ViewTableExtensions
    {
        public static IEnumerable<ViewTableRecord> GetViewTableRecords(this ViewTable symbolTbl, Transaction trx,
        OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return symbolTbl.GetSymbolTableRecords<ViewTableRecord>(trx, mode, filter, false);
        }

        public static IEnumerable<ViewTableRecord> GetViewTableRecords(this ViewTable symbolTbl,
    OpenMode mode = OpenMode.ForRead, SymbolTableRecordFilter filter = SymbolTableRecordFilter.None)
        {
            return
                symbolTbl.GetSymbolTableRecords<ViewTableRecord>(symbolTbl.Database.TransactionManager.TopTransaction,
                    mode, filter, false);
        }
    }
}
