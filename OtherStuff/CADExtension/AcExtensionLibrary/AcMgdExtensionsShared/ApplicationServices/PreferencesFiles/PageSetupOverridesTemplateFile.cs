namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class PageSetupOverridesTemplateFile
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
            get { return Preferences.Files.PageSetupOverridesTemplateFile; }
            set { Preferences.Files.PageSetupOverridesTemplateFile = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSetupOverridesTemplateFile"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public PageSetupOverridesTemplateFile(object acadPreferences)
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

    /// <summary>
    ///
    /// </summary>
    public class QNewTemplateFile
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
            get { return Preferences.Files.QNewTemplateFile; }
            set { Preferences.Files.QNewTemplateFile = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QNewTemplateFile"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public QNewTemplateFile(object acadPreferences)
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
}