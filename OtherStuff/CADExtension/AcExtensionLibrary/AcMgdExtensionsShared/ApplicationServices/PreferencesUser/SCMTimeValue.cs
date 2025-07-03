namespace Autodesk.AutoCAD.ApplicationServices.PreferencesUser
{
    /// <summary>
    ///
    /// </summary>
    public class SCMTimeValue
    {
        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        /// <value>
        /// The preferences.
        /// </value>
        protected dynamic Preferences { get; set; }

        /// <summary>
        /// Gets or sets the preference user.
        /// </summary>
        /// <value>
        /// The preference user.
        /// </value>
        protected virtual dynamic PreferenceUser
        {
            get { return Preferences.User.SCMTimeValue; }
            set { Preferences.User.SCMTimeValue = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SCMTimeValue"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public SCMTimeValue(object acadPreferences)
        {
            Preferences = acadPreferences;
        }

        /// <summary>
        /// Gets or sets the length of the time.
        /// </summary>
        /// <value>
        /// The length of the time.
        /// </value>
        public int TimeLength
        {
            get
            {
                return PreferenceUser;
            }
            set
            {
                if (value < 100)
                {
                    PreferenceUser = 100;
                }
                else if (value > 10000)
                {
                    PreferenceUser = 10000;
                }
                else
                {
                    PreferenceUser = value;
                }
            }
        }
    }
}