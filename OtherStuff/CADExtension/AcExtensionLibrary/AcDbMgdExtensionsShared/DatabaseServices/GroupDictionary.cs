namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    ///
    /// </summary>
    public class GroupDictionary : WrapperDBDictionary<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDictionary"/> class.
        /// </summary>
        /// <param name="trx">The TRX.</param>
        /// <param name="dic">The dic.</param>
        /// <param name="includingErased">if set to <c>true</c> [including erased].</param>
        public GroupDictionary(Transaction trx, DBDictionary dic, bool includingErased)
            : base(trx, dic, includingErased)
        {
            //add methods for dealing with groups
        }
    }
}