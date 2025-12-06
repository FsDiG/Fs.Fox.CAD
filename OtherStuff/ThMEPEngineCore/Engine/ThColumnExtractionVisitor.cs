using NFox.Cad;
using Linq2Acad;
using System.Linq;
using ThCADCore.NTS;
using ThCADExtension;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThMEPEngineCore.Algorithm;

namespace ThMEPEngineCore.Engine
{
    public class ThColumnExtractionVisitor : ThBuildingElementExtractionVisitor
    {
        public override void DoExtract(List<ThRawIfcBuildingElementData> elements, Entity dbObj, Matrix3d matrix)
        {
            if (dbObj is Hatch hatch)
            {
                elements.AddRange(HandleHatch(hatch, matrix));
            }
            else if (dbObj is Solid solid)
            {
                elements.AddRange(HandleSolid(solid, matrix));
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

        private List<ThRawIfcBuildingElementData> HandleHatch(Hatch hatch, Matrix3d matrix)
        {
            List<Curve> curves = new List<Curve>();
            if (IsBuildElement(hatch) && CheckLayerValid(hatch))
            {
                // 暂时不支持有“洞”的填充
                hatch.Boundaries().ForEachDbObject(o =>
                {
                    if (o is Polyline poly)
                    {
                        // 设计师会为矩形柱使用非比例的缩放
                        // 从而获得不同形状的矩形柱
                        // 考虑到多段线不能使用非比例的缩放
                        // 这里采用一个变通方法：
                        //  将矩形柱转化成实线，缩放后再转回多段线
                        if (poly.IsRectangle())
                        {
                            var solid = poly.ToSolid();
                            solid.TransformBy(matrix);
                            curves.Add(solid.ToPolyline());
                        }
                        else
                        {
                            poly.TransformBy(matrix);
                            curves.Add(poly);
                        }
                    }
                    else if (o is Circle circle)
                    {
                        // 圆形柱
                        var polyCircle = circle.ToPolyCircle();
                        polyCircle.TransformBy(matrix);
                        curves.Add(polyCircle);
                    }
                });
            }
            return curves.Select(o => CreateBuildingElementData(o)).ToList();
        }

        private List<ThRawIfcBuildingElementData> HandleSolid(Solid solid, Matrix3d matrix)
        {
            List<Curve> curves = new List<Curve>();
            if (IsBuildElement(solid) && CheckLayerValid(solid))
            {
                // 可能存在2D Solid不规范的情况
                // 这里将原始2d Solid“清洗”处理
                var clone = solid.WashClone();
                clone.TransformBy(matrix);
                curves.Add(clone.ToPolyline());
            }
            return curves.Select(o => CreateBuildingElementData(o)).ToList();
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
