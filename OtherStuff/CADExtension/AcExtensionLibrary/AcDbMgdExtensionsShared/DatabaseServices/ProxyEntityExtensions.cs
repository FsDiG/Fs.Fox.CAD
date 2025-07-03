namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public static class ProxyEntityExtensions
    {
        /// <summary>
        /// Determines whether this instance is erasable.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        public static bool IsErasable(this ProxyEntity proxy)
        {
            return (proxy.ProxyFlags & 1) == 1;
        }
    }
}