namespace Autodesk.AutoCAD.ApplicationServices
{
    /// <summary>
    ///
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Matcheses the specified pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static bool Matches(this string str, string pattern, bool ignoreCase = true)
        {
            return Autodesk.AutoCAD.Internal.Utils.WcMatchEx(str, pattern, ignoreCase);
        }
    }
}