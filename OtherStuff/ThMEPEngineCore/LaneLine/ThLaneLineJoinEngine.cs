using NFox.Cad;
using System.Linq;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.LaneLine
{
    public class ThLaneLineJoinEngine
    {
        public static DBObjectCollection Join(DBObjectCollection curves)
        {
            var objs = new DBObjectCollection();
            var kdTree = new ThCADCoreNTSKdTree(1.0);
            curves.Cast<Line>().ForEach(o => kdTree.InsertLine(o));
            curves.Cast<Line>().ForEach(o =>
            {
                objs.Add(new Line(kdTree.Query(o.StartPoint), kdTree.Query(o.EndPoint)));
            });
            return objs;
        }
    }
}
