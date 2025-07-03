using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NFox.Cad;
using Linq2Acad;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThExtractConnectPortsService : ThExtractService
    {
        public Dictionary<Polyline, string> ConnectPorts { get; private set; }
        public ThExtractConnectPortsService()
        {
            ConnectPorts = new Dictionary<Polyline, string>();
        }
        public override void Extract(Database db,Point3dCollection pts)
        {
            using (var acadDatabase = AcadDatabase.Use(db))
            {
                var boundaries = new List<Polyline>();
                var texts = new List<Entity>();
                foreach (var ent in acadDatabase.ModelSpace)
                {
                    if (IsConnectPortLayer(ent.Layer))
                    {
                        if (ent is Polyline polyline)
                        {
                            var newPolyline = polyline.Clone() as Polyline;
                            boundaries.Add(newPolyline);
                        }
                        else if (ent is DBText dbText)
                        {
                            texts.Add(dbText);
                        }
                        else if (ent is MText mText)
                        {
                            texts.Add(mText);
                        }
                    }
                }
                if (pts.Count >= 3)
                {
                    var boundarySpatialIndex = new ThCADCoreNTSSpatialIndex(boundaries.ToCollection());
                    var textsSpatialIndex = new ThCADCoreNTSSpatialIndex(texts.ToCollection());
                    boundaries = boundarySpatialIndex.SelectCrossingPolygon(pts).Cast<Polyline>().ToList();
                    texts = textsSpatialIndex.SelectCrossingPolygon(pts).Cast<Entity>().ToList();
                }
                var textSpatialIndex = new ThCADCoreNTSSpatialIndex(texts.ToCollection());
                boundaries.ForEach(o =>
                {
                    var selObjs = textSpatialIndex.SelectCrossingPolygon(o);
                    foreach (var item in selObjs)
                    {
                        if (item is DBText dbText)
                        {
                            if (ValidateText(dbText.TextString))
                            {
                                ConnectPorts.Add(o, dbText.TextString);
                                break;
                            }
                        }
                        else if (item is MText mText)
                        {
                            if (ValidateText(mText.Contents))
                            {
                                ConnectPorts.Add(o, mText.Contents);
                                break;
                            }
                        }
                    }
                });
            }
        }
        private bool ValidateText(string content)
        {
            string pattern = @"^[\d]+\s{0,}[A-Z]{1,}[\d]+";
            return Regex.IsMatch(content, pattern);
        }
        private bool IsConnectPortLayer(string layerName)
        {
            return layerName.ToUpper() == "连通";
        }
    }
}
