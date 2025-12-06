using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using ThMEPEngineCore.Model;

namespace ThMEPEngineCore.Engine
{
    public abstract class ThAnnotationElementRecognitionEngine : IDisposable
    {
        public List<ThIfcAnnotation> Elements { get; set; }
        public ThAnnotationElementRecognitionEngine()
        {
            Elements = new List<ThIfcAnnotation>();
        }

        public void Dispose()
        {
        }

        public abstract void Recognize(Database database, Point3dCollection polygon);

        public abstract void RecognizeMS(Database database, Point3dCollection polygon);

        public abstract void Recognize(List<ThRawIfcAnnotationElementData> datas, Point3dCollection polygon);
    }
}
