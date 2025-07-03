namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class TemplateDWGPath
    {
        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        /// <value>
        /// The preferences.
        /// </value>
        protected dynamic Preferences { get; set; }

        /// <summary>
        /// Gets or sets the preference file.
        /// </summary>
        /// <value>
        /// The preference file.
        /// </value>
        protected virtual dynamic PreferenceFile
        {
            get { return Preferences.Files.TemplateDWGPath; }
            set { Preferences.Files.TemplateDWGPath = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateDWGPath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public TemplateDWGPath(object acadPreferences)
        {
            Preferences = acadPreferences;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path
        {
            get { return PreferenceFile; }
            set { PreferenceFile = value; }
        }
    }

    //%appdata%\Autodesk\AutoCAD 2016\R20.1\enu\support\acad

    //%appdata%\Autodesk\AutoCAD 2016\R20.1\enu\support\acad
}