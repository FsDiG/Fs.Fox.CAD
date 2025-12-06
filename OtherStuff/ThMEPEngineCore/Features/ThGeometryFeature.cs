using System;
using ThCADCore.NTS;
using ThMEPEngineCore.Model;
using NetTopologySuite.Features;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;

namespace ThMEPEngineCore.Features
{
    public class ThGeometryFeature
    {
        public static Feature Construct(ThGeometry geometry)
        {
            if (geometry != null)
            {
                if (geometry.Boundary is Polyline polyline)
                {
                    var geo = polyline.ToNTSLineString();
                    var attributesTable = new AttributesTable(geometry.Properties);
                    var feature = new Feature(geo, attributesTable);
                    return feature;
                }
                else if (geometry.Boundary is Line line)
                {
                    var geo = line.ToNTSLineString();
                    var attributesTable = new AttributesTable(geometry.Properties);
                    var feature = new Feature(geo, attributesTable);
                    return feature;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return null;
        }
    }
}
