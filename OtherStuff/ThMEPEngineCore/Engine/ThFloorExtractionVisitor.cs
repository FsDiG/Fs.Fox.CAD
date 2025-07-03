using System.Linq;
using System.Collections.Generic;
using ThCADExtension;
using ThMEPEngineCore.CAD;
using ThMEPEngineCore.Algorithm;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Engine
{
    public class ThFloorExtractionVisitor : ThBuildingElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcBuildingElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if(dbObj is Polyline polyline)
            {
                elements.AddRange(Handle(polyline, matrix));
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

        private List<ThRawIfcBuildingElementData> Handle(Polyline polyline, Matrix3d matrix)
        {
            List<Curve> curves = new List<Curve>();
            if (IsBuildElement(polyline) && IsFloor(polyline))
            {
                var clone = polyline.WashClone();
                clone.TransformBy(matrix);
                curves.Add(clone);
            }
            return curves.Select(o => CreateBuildingElementData(o)).ToList();
        }

        private new bool IsBuildElement(Entity entity)
        {
            return entity.Hyperlinks.Count > 0;
        }

        private bool IsFloor(Curve curve)
        {
            var thPropertySet = ThPropertySet.CreateWithHyperlink(curve.Hyperlinks[0].Description);
            return thPropertySet.IsFloor;
        }

        private ThRawIfcBuildingElementData CreateBuildingElementData(Curve curve)
        {
            return new ThRawIfcBuildingElementData()
            {
                Geometry = curve,
            };
        }
    }
}
