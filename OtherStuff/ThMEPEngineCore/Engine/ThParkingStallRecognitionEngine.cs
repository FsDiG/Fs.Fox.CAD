using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Service;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Model;
using System.Collections.Generic;

namespace ThMEPEngineCore.Engine
{
    public class ThParkingStallRecognitionEngine : ThSpatialElementRecognitionEngine
    {
        public override void Recognize(Database database, Point3dCollection polygon)
        {
            var engine = new ThDbObjectRecognitionEngine();
            engine.Recognize(new ThParkingStallDbExtension(database), polygon);
            foreach(Curve curve in engine.DbObjects)
            {
                Spaces.Add(ThIfcParkingStall.Create(curve));
            }
        }

        public override void Recognize(List<ThRawIfcSpatialElementData> datas, Point3dCollection polygon)
        {
            throw new NotImplementedException();
        }

        public override void RecognizeMS(Database database, Point3dCollection polygon)
        {
            throw new NotImplementedException();
        }
    }
}
