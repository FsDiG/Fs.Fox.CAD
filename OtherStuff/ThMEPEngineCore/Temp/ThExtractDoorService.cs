using System;
using System.Linq;
using System.Collections.Generic;
using NFox.Cad;
using Linq2Acad;
using ThCADCore.NTS;
using ThMEPEngineCore.CAD;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThExtractDoorService : ThExtractService
    {
        public List<Polyline> Doors { get; set; }
        public ThExtractDoorService()
        {
            Doors = new List<Polyline>();
        }
        public override void Extract(Database db, Point3dCollection pts)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (ent is Polyline polyline)
                    {
                        if (IsDoorLayer(polyline.Layer))
                        {
                            var newPolyline = polyline.Clone() as Polyline;
                            Doors.Add(newPolyline);
                        }
                    }
                }
                if(pts.Count>=3)
                {
                    var spatialIndex = new ThCADCoreNTSSpatialIndex(Doors.ToCollection());
                    var objs = spatialIndex.SelectCrossingPolygon(pts);
                    Doors = objs.Cast<Polyline>().ToList();
                }                
                for (int i = 1; i <= Doors.Count; i++)
                {
                    var obb = Doors[i - 1].GetMinimumRectangle();
                    var rotatePts = DoorRotateAixPts(obb);
                    if (rotatePts.Count > 0)
                    {
                        var mt = Matrix3d.Rotation(Math.PI / 2.0 * Math.Pow(-1, i), Vector3d.ZAxis, rotatePts[0]);
                        Doors[i - 1].TransformBy(mt);
                    }
                }
            }
        }

        private List<Point3d> DoorRotateAixPts(Polyline polyline)
        {
            var results = new List<Point3d>();
            var lines = new List<Line>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var segment = polyline.GetLineSegmentAt(i);
                if (segment.Length > 5.0)
                {
                    lines.Add(new Line(segment.StartPoint, segment.EndPoint));
                }
            }
            lines = lines.OrderBy(o => o.Length).ToList();
            results.Add(lines[0].StartPoint.GetMidPt(lines[0].EndPoint));
            results.Add(lines[1].StartPoint.GetMidPt(lines[1].EndPoint));
            return results;
        }

        private bool IsDoorLayer(string layerName)
        {
            return layerName == "门";
        }
    }
}
