using System.Collections.Generic;
using System.Linq;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class SymbolTableRecordExtensions
    {
        /// <summary>
        /// Nameses the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IEnumerable<string> Names(this IEnumerable<SymbolTableRecord> source)
        {
            return source.Select(str => str.Name);
        }
    }
}