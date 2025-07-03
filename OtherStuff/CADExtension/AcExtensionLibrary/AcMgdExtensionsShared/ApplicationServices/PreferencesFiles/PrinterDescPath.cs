namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class PrinterDescPath : SupportPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterDescPath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public PrinterDescPath(object acadPreferences)
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
            get { return Preferences.Files.PrinterDescPath; }
            set { Preferences.Files.PrinterDescPath = value; }
        }
    }
}