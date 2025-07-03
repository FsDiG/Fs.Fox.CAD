using System;

namespace Autodesk.AutoCAD.ApplicationServices.PreferencesFiles
{
    /// <summary>
    ///
    /// </summary>
    public class ToolPalettePath : SupportPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolPalettePath"/> class.
        /// </summary>
        /// <param name="acadPreferences">The acad preferences.</param>
        public ToolPalettePath(object acadPreferences) : base(acadPreferences)
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
            get { return Preferences.Files.ToolPalettePath; }
            set { Preferences.Files.ToolPalettePath = value; }
        }

        /// <summary>
        /// The _default path
        /// </summary>
        private static string _defaultPath =
            Environment.ExpandEnvironmentVariables(@"%AppData%\Autodesk\AutoCAD 2016\R20.1\enu\Support\ToolPalette");

        /// <summary>
        /// Gets the default path.
        /// </summary>
        /// <value>
        /// The default path.
        /// </value>
        public static string DefaultPath { get { return _defaultPath; } }

        //
    }
}