using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ThMEPEngineCore.IO;
using ThMEPEngineCore.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Temp
{
    public class ThExtractGeometryEngine : IDisposable,IBuildGeometry
    { 
        private List<ThExtractorBase> Extrators { get; set; }
        private List<IPrint> Printers { get; set; }
        public ThExtractGeometryEngine()
        {
            Extrators = new List<ThExtractorBase>();
        }
        public void Accept(ThExtractorBase extractor)
        {
            var count = Extrators.Where(o => o.GetType() == extractor.GetType()).Count();
            if(count==0)
            {
                Extrators.Add(extractor);
            }
        }
        public void Accept(List<ThExtractorBase> extractors)
        {
            extractors.ForEach(o => Accept(o));
        }
        public void Dispose()
        {            
        }
        public void Extract(Database database, Point3dCollection pts)
        {
            Extrators.ForEach(o => (o as IExtract).Extract(database, pts));
        }     
        public void Print(Database database)
        {
            Extrators.ForEach(o => (o as IPrint).Print(database));
        }

        public void OutputGeo(string activeDocName)
        {
            // 输出GeoJson文件
            var fileInfo = new FileInfo(activeDocName);
            var path = fileInfo.Directory.FullName;
            var geos = BuildGeometries();
            ThGeoOutput.Output(geos, path, fileInfo.Name);
        }

        public List<ThGeometry> BuildGeometries()
        {
            var geos = new List<ThGeometry>();
            Extrators.ForEach(o => geos.AddRange((o as IBuildGeometry).BuildGeometries()));
            return geos;
        }
    }
}
