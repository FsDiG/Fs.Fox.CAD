using Autodesk.AutoCAD.ApplicationServices.PreferencesDisplay;
using Autodesk.AutoCAD.ApplicationServices.PreferencesFiles;
using Autodesk.AutoCAD.ApplicationServices.PreferencesUser;

namespace Autodesk.AutoCAD.ApplicationServices
{
    /// <summary>
    ///
    /// </summary>
    public static class Preferences
    {
        /// <summary>
        /// Gets the acad preferences.
        /// </summary>
        /// <value>
        /// The acad preferences.
        /// </value>
        private static dynamic AcadPreferences
        { get { return Application.Preferences; } }

        /// <summary>
        /// Gets the support paths.
        /// </summary>
        /// <value>
        /// The support paths.
        /// </value>
        public static SupportPath SupportPaths { get { return new SupportPath(AcadPreferences); } }

        /// <summary>
        /// Gets the tool palette paths.
        /// </summary>
        /// <value>
        /// The tool palette paths.
        /// </value>
        public static ToolPalettePath ToolPalettePaths { get { return new ToolPalettePath(AcadPreferences); } }

        /// <summary>
        /// Gets the printer configuration path.
        /// </summary>
        /// <value>
        /// The printer configuration path.
        /// </value>
        public static PrinterConfigPath PrinterConfigPath { get { return new PrinterConfigPath(AcadPreferences); } }

        /// <summary>
        /// Gets the printer desc path.
        /// </summary>
        /// <value>
        /// The printer desc path.
        /// </value>
        public static PrinterDescPath PrinterDescPath { get { return new PrinterDescPath(AcadPreferences); } }

        /// <summary>
        /// Gets the printer style sheet path.
        /// </summary>
        /// <value>
        /// The printer style sheet path.
        /// </value>
        public static PrinterStyleSheetPath PrinterStyleSheetPath { get { return new PrinterStyleSheetPath(AcadPreferences); } }

        /// <summary>
        /// Gets the template DWG path.
        /// </summary>
        /// <value>
        /// The template DWG path.
        /// </value>
        public static TemplateDWGPath TemplateDWGPath { get { return new TemplateDWGPath(AcadPreferences); } }

        /// <summary>
        /// Gets the enterprise menu file.
        /// </summary>
        /// <value>
        /// The enterprise menu file.
        /// </value>
        public static EnterpriseMenuFile EnterpriseMenuFile { get { return new EnterpriseMenuFile(AcadPreferences); } }

        /// <summary>
        /// Gets the menu file.
        /// </summary>
        /// <value>
        /// The menu file.
        /// </value>
        public static MenuFile MenuFile { get { return new MenuFile(AcadPreferences); } }

        /// <summary>
        /// Gets the display scroll bars.
        /// </summary>
        /// <value>
        /// The display scroll bars.
        /// </value>
        public static DisplayScrollBars DisplayScrollBars { get { return new DisplayScrollBars(AcadPreferences); } }

        /// <summary>
        /// Gets the SCM time value.
        /// </summary>
        /// <value>
        /// The SCM time value.
        /// </value>
        public static SCMTimeValue SCMTimeValue { get { return new SCMTimeValue(AcadPreferences); } }

        /// <summary>
        /// Gets the page setup overrides template file.
        /// </summary>
        /// <value>
        /// The page setup overrides template file.
        /// </value>
        public static PageSetupOverridesTemplateFile PageSetupOverridesTemplateFile { get { return new PageSetupOverridesTemplateFile(AcadPreferences); } }

        /// <summary>
        /// Gets the q new template file.
        /// </summary>
        /// <value>
        /// The q new template file.
        /// </value>
        public static QNewTemplateFile QNewTemplateFile { get { return new QNewTemplateFile(AcadPreferences); } }
    }
}