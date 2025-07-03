using System;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public abstract class ThSpatialElementRecognitionEngine : IDisposable
    {
        public List<ThIfcSpace> Spaces { get; set; }
        public List<ThIfcSpatialElement> Elements { get; set; }
        public ThSpatialElementRecognitionEngine()
        {
            Spaces = new List<ThIfcSpace>();
            Elements = new List<ThIfcSpatialElement>();
        }
        public void Dispose()
        {
        }

        public abstract void Recognize(Database database, Point3dCollection polygon);

        public abstract void RecognizeMS(Database database, Point3dCollection polygon);

        public abstract void Recognize(List<ThRawIfcSpatialElementData> datas, Point3dCollection polygon);
    }
}
