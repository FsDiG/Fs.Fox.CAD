using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.CAD
{
    public static class ThXrefDbExtension
    {
        public static List<string> XrefPaths(this Database db)
        {
            var xrefs = new List<string>();
            XrefGraph xrg = db.GetHostDwgXrefGraph(false);
            for (int i = 1; i < xrg.NumNodes; i++)
            {
                XrefGraphNode xrgn = xrg.GetXrefNode(i);
                if (!xrgn.IsNested)
                {
                    xrefs.Add(xrgn.XRefPath());
                    if (xrgn.XrefStatus == XrefStatus.Resolved)
                    {
                        xrefs.AddRange(xrgn.Database.XrefPaths());
                    }
                }
            }
            return xrefs;
        }

        public static string XRefPath(this XrefGraphNode xrefGraphNode)
        {
            var block = xrefGraphNode.BlockTableRecordId;
            using (AcadDatabase acadDatabase = AcadDatabase.Use(block.Database))
            {
                return acadDatabase.Blocks.Element(block).PathName;
            }
        }
    }
}