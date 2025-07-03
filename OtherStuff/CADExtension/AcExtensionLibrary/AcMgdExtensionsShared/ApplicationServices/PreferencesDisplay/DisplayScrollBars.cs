namespace Autodesk.AutoCAD.ApplicationServices.PreferencesDisplay
{
    /// <summary>
    ///
    /// </summary>
    public class DisplayScrollBars
    {
        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        /// <value>
        /// The preferences.
        /// </value>
        protected dynamic Preferences { get; set; }

        /// <summary>
        /// Gets or sets the preference display.
        /// </summary>
        /// <value>
        /// The preference display.
        /// </value>
        protected virtual dynamic PreferenceDisplay
        {
            get { return Preferences.Display.DisplayScrollBars; }
            set { Preferences.Display.DisplayScrollBars = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayScrollBars"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public DisplayScrollBars(object acadPreferences)
        {
            Preferences = acadPreferences;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DisplayScrollBars"/> is display.
        /// </summary>
        /// <value>
        ///   <c>true</c> if display; otherwise, <c>false</c>.
        /// </value>
        public bool Display
        {
            get { return PreferenceDisplay; }
            set { PreferenceDisplay = value; }
        }
    }
}