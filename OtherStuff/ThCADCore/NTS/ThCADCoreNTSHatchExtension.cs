using System;
using System.Linq;
using ThCADExtension;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSHatchExtension
    {
        private static Geometry ToNTSGeometry(this Hatch hatch)
        {
            var polygons = new List<Polygon>();
            hatch.Boundaries().ForEach(o => polygons.Add(o.ToNTSPolygon()));
            ThCADCoreNTSBuildArea buildArea = new ThCADCoreNTSBuildArea();
            return buildArea.Build(ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(polygons.ToArray()));
        }

        private static MultiPolygon ToNTSMultiPolygon(this Hatch hatch)
        {
            var geometry = hatch.ToNTSGeometry();
            if (geometry.IsEmpty)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon();
            }
            if (geometry is Polygon polygon)
            {
                var polygons = new Polygon[] { polygon };
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateMultiPolygon(polygons);
            }
            else if (geometry is MultiPolygon multiPolygon)
            {
                return multiPolygon;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public static List<Polygon> ToPolygons(this Hatch hatch)
        {
            var objs = new List<Polygon>();
            hatch.ToNTSMultiPolygon().Geometries
                .Cast<Polygon>()
                .ForEach(o => objs.Add(o));
            return objs;
        }
        public static List<MPolygon> ToMPolygons(this Hatch hatch)
        {
            // 在AutoCAD 2012下，新创建的MPolygon还不能立即使用。
            // 需要把创建的MPolygon添加到Database后才可以正常使用。
            // 代码例子：
            //  acadDatabase.ModelSpace.Add(mPolygon);
            //  mPolygon.TransformBy(matrix);
            //  .....
            //  mPolygon.Erase();
            var objs = new List<MPolygon>();
            hatch.ToNTSMultiPolygon().Geometries
                .Cast<Polygon>()
                .ForEach(o => objs.Add(o.ToDbMPolygon()));
            return objs;
        }
    }
}
