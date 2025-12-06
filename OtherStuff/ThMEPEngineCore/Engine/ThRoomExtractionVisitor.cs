using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThRoomExtractionVisitor : ThSpatialElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcSpatialElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is Polyline polyline)
            {
                elements.AddRange(Handle(polyline, matrix));
            }
        }

        public override void DoExtract(List<ThRawIfcSpatialElementData> elements, Entity dbObj)
        {
            if (dbObj is Polyline polyline)
            {
                elements.AddRange(Handle(polyline));
            }
        }

        public override void DoXClip(List<ThRawIfcSpatialElementData> elements, BlockReference blockReference, Matrix3d matrix)
        {
            var xclip = blockReference.XClipInfo();
            if (xclip.IsValid)
            {
                xclip.TransformBy(matrix);
                elements.RemoveAll(o => !xclip.Contains(o.Geometry as Curve));
            }
        }

        private List<ThRawIfcSpatialElementData> Handle(Polyline polyline, Matrix3d matrix)
        {
            var results = new List<ThRawIfcSpatialElementData>();
            if (IsSpatialElement(polyline) && CheckLayerValid(polyline))
            {
                var clone = polyline.WashClone();
                clone.TransformBy(matrix);
                results.Add(CreateSpatialElementData(clone, polyline.Hyperlinks[0].Description));
            }
            return results;
        }

        private List<ThRawIfcSpatialElementData> Handle(Polyline polyline)
        {
            var results = new List<ThRawIfcSpatialElementData>();
            if (CheckLayerValid(polyline))
            {
                var clone = polyline.WashClone();
                results.Add(CreateSpatialElementData(clone, ""));
            }
            return results;
        }

        private ThRawIfcSpatialElementData CreateSpatialElementData(Curve curve, string description)
        {
            return new ThRawIfcSpatialElementData()
            {
                Geometry = curve,
                Data = description
            };
        }

        public override bool IsSpatialElement(Entity entity)
        {
            if (entity.Hyperlinks.Count > 0)
            {
                var thPropertySet = ThPropertySet.CreateWithHyperlink2(entity.Hyperlinks[0].Description);
                if (thPropertySet.Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_CATEGORY) &&
                    thPropertySet.Properties.ContainsKey(ThMEPEngineCoreCommon.BUILDELEMENT_PROPERTY_Boundary))
                {
                    return true;
                }
            }
            return false;
        }    
    }
}
