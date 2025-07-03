using Autodesk.AutoCAD.Runtime;

namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// ObjectOverule
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class OsnapOverrule<T> : OsnapOverrule where T : Entity
    {
        /// <summary>
        /// The _target class
        /// </summary>
        private readonly RXClass _targetClass = RXObject.GetClass(typeof(T));

        /// <summary>
        /// The _status
        /// </summary>
        private OverruleStatus _status = OverruleStatus.Off;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public OverruleStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value == OverruleStatus.On && _status == OverruleStatus.Off)
                {
                    Overrule.AddOverrule(_targetClass, this, true);
                    _status = OverruleStatus.On;
                }
                else if (value == OverruleStatus.Off && _status == OverruleStatus.On)
                {
                    Overrule.RemoveOverrule(_targetClass, this);
                    _status = OverruleStatus.Off;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OsnapOverrule{T}"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        protected OsnapOverrule(OverruleStatus status = OverruleStatus.On)
        {
            Status = status;
        }
    }
}