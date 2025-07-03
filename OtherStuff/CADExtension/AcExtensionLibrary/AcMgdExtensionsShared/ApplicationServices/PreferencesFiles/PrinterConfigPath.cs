namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class PrinterConfigPath : SupportPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterConfigPath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public PrinterConfigPath(object acadPreferences)
            : base(acadPreferences)
        {
        }

        /// <summary>
        /// Gets or sets the preferences file.
        /// </summary>
        /// <value>
        /// The preferences file.
        /// </value>
        protected override dynamic PreferencesFile
        {
            get { return Preferences.Files.PrinterConfigPath; }
            set { Preferences.Files.PrinterConfigPath = value; }
        }
    }
}