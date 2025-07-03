using System;
using System.Linq;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;

namespace ThMEPEngineCore.Engine
{
    public class ThRawDoorStone : ThRawIfcBuildingElementData
    {
        //
    }

    public class ThDoorStoneExtractionVisitor : ThBuildingElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcBuildingElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is Polyline polyline)
            {
                elements.AddRange(HandlePolyline(polyline, matrix));
            }
        }

        public override void DoXClip(List<ThRawIfcBuildingElementData> elements, BlockReference blockReference, Matrix3d matrix)
        {
            var xclip = blockReference.XClipInfo();
            if (xclip.IsValid)
            {
                xclip.TransformBy(matrix);
                elements.RemoveAll(o => !xclip.Contains(o.Geometry as Curve));                
            }
        }

        private List<ThRawIfcBuildingElementData> HandlePolyline(Polyline polyline, Matrix3d matrix)
        {
            List<Curve> curves = new List<Curve>();
            if (IsBuildElement(polyline) && IsDoorStone(polyline))
            {
                var clone = polyline.WashClone();
                clone.TransformBy(matrix);
                curves.Add(clone);
            }
            return curves.Select(o => CreateBuildingElementData(o)).ToList();
        }

        private ThRawIfcBuildingElementData CreateBuildingElementData(Curve curve)
        {
            return new ThRawDoorStone()
            {
                Geometry = curve,
            };
        }
        private new bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }
        private bool IsDoorStone(Curve curve)
        {
            var thPropertySet = ThPropertySet.CreateWithHyperlink(curve.Hyperlinks[0].Description);
            return thPropertySet.IsDoor;
        }        
    }
}
