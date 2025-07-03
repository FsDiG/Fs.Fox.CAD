using Autodesk.AutoCAD.DatabaseServices;

namespace Autodesk.AutoCAD.ApplicationServices
{
    /// <summary>
    ///
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Author: Tony Tanzillo
        /// Source: http://www.theswamp.org/index.php?topic=42016.msg471429#msg471429
        /// </summary>
        /// <param name="doc">The document.</param>
        public static void Save(this Document doc)
        {
#if ACAD2012
            dynamic acadDoc = doc.AcadDocument;
#else
            dynamic acadDoc = doc.GetAcadDocument();
#endif
            acadDoc.Save();
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="command">The command.</param>
        public static void SendCommand(this Document doc, string command)
        {
#if ACAD2012
            dynamic acadDoc = doc.AcadDocument;
#else
            dynamic acadDoc = doc.GetAcadDocument();
#endif
            acadDoc.SendCommand(command);
        }

        /// <summary>
        /// Sends the cancel.
        /// </summary>
        /// <param name="doc">The document.</param>
        public static void SendCancel(this Document doc)
        {
            doc.SendCommand("\x03\x03");
        }

/*
#if (!ACAD2013 && !ACAD2014)
        /// <summary>
        /// Creates the preview icon.
        /// </summary>
        /// <param name="btr">The BTR.</param>
        public static void CreatePreviewIcon(this BlockTableRecord btr)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.Command("_.BLOCKICON", btr.Name);
        }
#endif
*/

    }
}