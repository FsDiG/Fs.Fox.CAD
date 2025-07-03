namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class ProxyObjectExtensions
    {
        /// <summary>
        /// Determines whether this instance is erasable.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        public static bool IsErasable(this ProxyObject proxy)
        {
            return (proxy.ProxyFlags & 1) == 1;
        }
    }
}