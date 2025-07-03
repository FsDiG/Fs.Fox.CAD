using AcHelper;
using Linq2Acad;
using ThCADExtension;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMEPEngineCore.Service.Hvac
{
    public static class ThFanSelectionEditorExtension
    {
        public static void ZoomToModels(this Editor ed, Entity[] entities, double scale)
        {
            try
            {
                var extents = new Extents3d();
                entities.ForEachDbObject(e =>
                {
                    Plane XYPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
                    Matrix3d matrix = Matrix3d.Projection(XYPlane, XYPlane.Normal);
                    var projectExtents = new Extents3d(
                        e.GeometricExtents.MinPoint.TransformBy(matrix),
                        e.GeometricExtents.MaxPoint.TransformBy(matrix));
                    extents.AddExtents(projectExtents);
                });
                //wcs下的Extend中心点
                var center = extents.CenterPoint();
                //Extend的宽和高
                double projExtendHeight = extents.Height();
                double projExtendWidth = extents.Width();
                //以Extentd中心为ocs坐标原点，获取ocs下的Extend的最大最小点坐标
                Point3d ocsMinPoint = new Point3d(-0.5 * projExtendWidth, -0.5 * projExtendHeight, 0);
                Point3d ocsMaxPoint = new Point3d(0.5 * projExtendWidth, 0.5 * projExtendHeight, 0);
                var ocsExtents = new Extents3d(ocsMinPoint, ocsMaxPoint);
                // 转换到WCS
                Matrix3d rotationMat = Matrix3d.Identity;
                Matrix3d scaleMat = Matrix3d.Scaling(scale, Point3d.Origin);
                Matrix3d movementMat = Matrix3d.Displacement(center.GetAsVector());
                ocsExtents.TransformBy(scaleMat.PreMultiplyBy(rotationMat).PreMultiplyBy(movementMat));
                // 在WCS下缩放到范围内
                COMTool.ZoomWindow(ocsExtents.MinPoint, ocsExtents.MaxPoint);
            }
            catch
            {
                // 在缺失字体的情况下，BlockReference.GeometricExtents会抛出eInvalidExtents异常
                // 若异常发生，不执行任何操作
            }
        }

        public static void PickFirstModels(this Editor ed, ObjectId[] objs)
        {
            // 首先清空现有的PickFirst选择集
            Active.Editor.SetImpliedSelection(new ObjectId[0]);
            // 接着讲模型添加到PickFirst选择集
            Active.Editor.SetImpliedSelection(objs);
        }
    }
}
