namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class PrinterStyleSheetPath : SupportPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterStyleSheetPath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public PrinterStyleSheetPath(object acadPreferences)
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
            get { return Preferences.Files.PrinterStyleSheetPath; }
            set { Preferences.Files.PrinterStyleSheetPath = value; }
        }
    }
}