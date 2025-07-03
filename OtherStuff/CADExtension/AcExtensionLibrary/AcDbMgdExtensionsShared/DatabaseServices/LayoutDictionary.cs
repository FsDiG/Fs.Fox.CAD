namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public class LayoutDictionary : WrapperDBDictionary<Layout>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutDictionary"/> class.
        /// </summary>
        /// <param name="trx">The TRX.</param>
        /// <param name="dic">The dic.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        public LayoutDictionary(Transaction trx, DBDictionary dic, bool includingErased)
            : base(trx, dic, includingErased)
        {
            //add methods for dealing with groups
        }
    }
}