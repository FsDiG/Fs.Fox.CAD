using AcHelper;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using ThMEPEngineCore.BeamInfo;
using ThMEPEngineCore.BeamInfo.Model;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad;

namespace ThMEPEngineCore.Engine
{
    public class ThRawBeamExtractionEngine : ThBuildingElementExtractionEngine
    {
        public override void Extract(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            using (var beamDbExtension = new ThStructureBeamDbExtension(database))
            {
                //获取梁线
                beamDbExtension.BuildElementCurves();
                Results = beamDbExtension.BeamCurves.Select(o => new ThRawIfcBuildingElementData()
                {
                    Geometry = o,
                }).ToList();
            }

        }
    }

    public class ThRawBeamRecognitionEngine : ThBuildingElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThRawBeamExtractionEngine();
            engine.Extract(database);
            Recognize(engine.Results, polygon);
        }

        public override void Recognize(List<ThRawIfcBuildingElementData> datas, Point3dCollection polygon)
        {
            List<Curve> curves = new List<Curve>();
            var objs = datas.Select(o => o.Geometry).ToCollection();
            if (polygon.Count > 0)
            {
                ThCADCoreNTSSpatialIndex beamCurveSpatialIndex = new ThCADCoreNTSSpatialIndex(objs);
                foreach (var filterObj in beamCurveSpatialIndex.SelectCrossingPolygon(polygon))
                {
                    curves.Add(filterObj as Curve);
                }
            }
            else
            {
                curves = objs.Cast<Curve>().ToList();
            }
            //获取梁段
            DBObjectCollection beamCollection = new DBObjectCollection();
            curves.ForEach(o => beamCollection.Add(o));
            ThDistinguishBeamInfo thDisBeamInfo = new ThDistinguishBeamInfo();
            thDisBeamInfo.CalBeamStruc(beamCollection).Cast<LineBeam>().ForEach(o =>
            {
                Elements.Add(ThIfcLineBeam.Create(o.BeamBoundary));
            });
        }     
    }
}