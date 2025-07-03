namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class LayerTableRecordExtensions
    {
        /// <summary>
        /// Layers the name typed value.
        /// </summary>
        /// <param name="ent">The ent.</param>
        /// <returns></returns>
        public static TypedValue LayerNameTypedValue(this Entity ent)
        {
            return new TypedValue((int)TypeCode.LayerName, ent.Layer);
        }
    }
}