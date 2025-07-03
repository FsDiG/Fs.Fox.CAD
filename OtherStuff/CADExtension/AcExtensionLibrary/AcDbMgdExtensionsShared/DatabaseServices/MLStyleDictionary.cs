namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public class MLStyleDictionary : WrapperDBDictionary<MLeaderStyle>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MLStyleDictionary"/> class.
        /// </summary>
        /// <param name="trx">The TRX.</param>
        /// <param name="dic">The dic.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        public MLStyleDictionary(Transaction trx, DBDictionary dic, bool includingErased)
            : base(trx, dic, includingErased)
        {
            //add methods for dealing with Mleaders
        }
    }
}