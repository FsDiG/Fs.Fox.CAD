using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace DotNetARX
{
    /// <summary>
    /// 3D操作类
    /// </summary>
    public static partial class Draw3DTools
    {
        /// <summary>
        /// 由角点、长度、宽度和高度在UCS中创建长方体
        /// </summary>
        /// <param name="cornerPt">角点</param>
        /// <param name="lengthX">长度</param>
        /// <param name="lengthY">宽度</param>
        /// <param name="lengthZ">高度</param>
        /// <returns>返回创建的长方体的Id</returns>
        public static ObjectId AddBox(Point3d cornerPt, double lengthX,
            double lengthY, double lengthZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(lengthX) < 0.00001 || Math.Abs(lengthY) < 0.00001 ||
                Math.Abs(lengthZ) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建长方体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateBox(Math.Abs(lengthX), Math.Abs(lengthY), Math.Abs(lengthZ));

            // 位置调整
            Point3d cenPt = cornerPt + new Vector3d(0.5 * lengthX, 0.5 * lengthY, 0.5 * lengthZ);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }          
            return entId;
        }

        /// <summary>
        /// 由底面中心点、半径和高度在UCS中创建圆柱体
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="radius">底面半径</param>
        /// <param name="height">高度</param>
        /// <returns>返回创建的圆柱体的Id</returns>
        public static ObjectId AddCylinder(Point3d bottomCenPt, double radius, double height)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (radius < 0.00001 || Math.Abs(height) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建圆柱体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateFrustum(Math.Abs(height), radius, radius, radius);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由底面中心点、半径和高度在UCS中创建圆锥体
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="radius">底面半径</param>
        /// <param name="height">高度</param>
        /// <returns>返回创建的圆锥体的Id</returns>
        public static ObjectId AddCone(Point3d bottomCenPt, double radius, double height)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (radius < 0.00001 || Math.Abs(height) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建圆锥体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateFrustum(Math.Abs(height), radius, radius, 0);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);

            if (height < 0)
            {
                Plane miPlane = new Plane(bottomCenPt, bottomCenPt + new Vector3d(radius, 0.0, 0.0),
                bottomCenPt + new Vector3d(0.0, radius, 0.0));
                Matrix3d mtMirroring = Matrix3d.Mirroring(miPlane);
                mt = mt * Matrix3d.Mirroring(miPlane);
            }

            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由中心点和半径在UCS中创建球体
        /// </summary>
        /// <param name="cenPt">中心点</param>
        /// <param name="radius">半径</param>
        /// <returns>返回创建的球体的Id</returns>
        public static ObjectId AddSphere(Point3d cenPt, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (radius < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建球体失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateSphere(radius);

            // 位置调整
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由中心点、圆环半径和圆管半径在UCS中创建圆环体
        /// </summary>
        /// <param name="cenPt">中心点</param>
        /// <param name="majorRadius">圆环半径</param>
        /// <param name="minorRadius">圆管半径</param>
        /// <returns>返回创建的圆环体的Id</returns>
        public static ObjectId AddTorus(Point3d cenPt, double majorRadius, double minorRadius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(majorRadius) < 0.00001 || minorRadius < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建圆锥体失败！");
                return ObjectId.Null;
            }

            try
            {
                // 创建
                Solid3d ent = new Solid3d();
                ent.RecordHistory = true;
                ent.CreateTorus(majorRadius, minorRadius);

                // 位置调整
                Matrix3d mt = ed.CurrentUserCoordinateSystem;
                mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
                ent.TransformBy(mt);

                ObjectId entId = ObjectId.Null;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    tr.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当,创建圆锥体失败！");
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 由角点、长度、宽度和高度在UCS中创建楔体
        /// </summary>
        /// <param name="cornerPt">角点</param>
        /// <param name="lengthAlongX">长度</param>
        /// <param name="lengthAlongY">宽度</param>
        /// <param name="lengthAlongZ">高度</param>
        /// <returns>返回创建的楔体的Id</returns>
        public static ObjectId AddWedge(Point3d cornerPt, double lengthAlongX,
            double lengthAlongY, double lengthAlongZ)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(lengthAlongX) < 0.00001 || Math.Abs(lengthAlongX) < 0.00001 || Math.Abs(lengthAlongX) < 0.00001)
            {
                ed.WriteMessage("\n参数不当,创建楔体失败！");
                return ObjectId.Null;
            }

            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreateWedge(Math.Abs(lengthAlongX), Math.Abs(lengthAlongY), Math.Abs(lengthAlongZ));

            // 位置调整
            Point3d cenPt = cornerPt + new Vector3d(0.5 * lengthAlongX, 0.5 * lengthAlongY, 0.5 * lengthAlongZ);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由底面中心点、高度、棱数和底面外接圆半径在UCS中创建棱柱
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="height">高度</param>
        /// <param name="sides">棱数</param>
        /// <param name="radius">底面外接圆半径</param>
        /// <returns>返回创建的棱柱的Id</returns>
        public static ObjectId AddPrism(Point3d bottomCenPt, double height, int sides, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(height) < 0.00001 || radius < 0.00001 || sides < 3 || sides > 32)
            {
                ed.WriteMessage("\n参数不当,创建棱柱失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreatePyramid(Math.Abs(height), sides, radius, radius);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);
            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由底面中心点、高度、棱数和底面外接圆半径创建棱锥
        /// </summary>
        /// <param name="bottomCenPt">底面中心点</param>
        /// <param name="height">高度</param>
        /// <param name="sides">棱数</param>
        /// <param name="radius">底面外接圆半径</param>
        /// <returns>返回创建的棱锥的Id</returns>
        public static ObjectId AddPyramid(Point3d bottomCenPt, double height, int sides, double radius)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (Math.Abs(height) < 0.00001 || radius < 0.00001 || sides < 3 || sides > 32)
            {
                ed.WriteMessage("\n参数不当,创建棱柱失败！");
                return ObjectId.Null;
            }

            // 创建
            Solid3d ent = new Solid3d();
            ent.RecordHistory = true;
            ent.CreatePyramid(Math.Abs(height), sides, radius, 0);

            // 位置调整
            Point3d cenPt = bottomCenPt + new Vector3d(0.0, 0.0, 0.5 * height);
            Matrix3d mt = ed.CurrentUserCoordinateSystem;
            mt = mt * Matrix3d.Displacement(cenPt - Point3d.Origin);

            if (height < 0)
            {
                Plane miPlane = new Plane(bottomCenPt, bottomCenPt + new Vector3d(radius, 0.0, 0.0),
                bottomCenPt + new Vector3d(0.0, radius, 0.0));
                Matrix3d mtMirroring = Matrix3d.Mirroring(miPlane);
                mt = mt * Matrix3d.Mirroring(miPlane);
            }

            ent.TransformBy(mt);

            ObjectId entId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                entId = db.AddToModelSpace(ent);
                tr.Commit();
            }
            return entId;
        }

        /// <summary>
        /// 由截面面域、拉伸高度和拉伸角度创建拉伸体
        /// </summary>
        /// <param name="region">截面面域</param>
        /// <param name="height">拉伸高度</param>
        /// <param name="taperAngle">拉伸角度</param>
        /// <returns>返回创建的拉伸体的Id</returns>
        public static ObjectId AddExtrudedSolid(Region region, double height,
            double taperAngle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                Solid3d ent = new Solid3d();
                ent.Extrude(region, height, taperAngle);

                ObjectId entId = ObjectId.Null;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    tr.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当,创建拉伸体失败！");
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 由截面面域、拉伸路径曲线和拉伸角度创建拉伸体
        /// </summary>
        /// <param name="region">截面面域</param>
        /// <param name="path">拉伸路径曲线</param>
        /// <param name="taperAngle">拉伸角度</param>
        /// <returns>返回创建的拉伸体的Id</returns>
        public static ObjectId AddExtrudedSolid(Region region, Curve path,
            double taperAngle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                Solid3d ent = new Solid3d();
                ent.ExtrudeAlongPath(region, path, taperAngle);

                ObjectId entId = ObjectId.Null;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    tr.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当,创建拉伸体失败！");
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 由截面面域、旋转轴起点、旋转轴终点和旋转角度创建旋转体
        /// </summary>
        /// <param name="region">截面面域</param>
        /// <param name="axisPt1">旋转轴起点</param>
        /// <param name="axisPt2">旋转轴终点</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>返回创建的旋转体的Id</returns>
        public static ObjectId AddRevolvedSolid(Region region, Point3d axisPt1,
            Point3d axisPt2, double angle)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                Solid3d ent = new Solid3d();
                ent.Revolve(region, axisPt1, axisPt2 - axisPt1, angle);

                ObjectId entId = ObjectId.Null;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    entId = db.AddToModelSpace(ent);
                    tr.Commit();
                }
                return entId;
            }
            catch
            {
                ed.WriteMessage("\n参数不当,创建旋转体失败！");
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 由布尔操作函数创建三维实体
        /// </summary>
        /// <param name="boolType">布尔操作类型</param>
        /// <param name="solid3dId1">参与操作的三维实体的Id</param>
        /// <param name="solid3dId2">参与操作的三维实体的Id</param>
        /// <returns>返回创建的三维实体的Id</returns>
        public static bool BoolSolid3dRegion(BooleanOperationType boolType,
            ObjectId solid3dId1, ObjectId solid3dId2)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity ent1 = trans.GetObject(solid3dId1, OpenMode.ForWrite) as Entity;
                    Entity ent2 = trans.GetObject(solid3dId2, OpenMode.ForWrite) as Entity;

                    if (ent1 == null || ent2 == null)
                    {
                        ed.WriteMessage("\n布尔操作失败！");
                        return false;
                    }

                    if (ent1 is Solid3d & ent2 is Solid3d)
                    {
                        Solid3d solid3dEnt1 = (Solid3d)ent1;
                        Solid3d solid3dEnt2 = (Solid3d)ent2;
                        solid3dEnt1.BooleanOperation(boolType, solid3dEnt2);
                        ent2.Dispose();
                    }

                    if (ent1 is Region & ent2 is Region)
                    {
                        Region regionEnt1 = (Region)ent1;
                        Region regionEnt2 = (Region)ent2;
                        regionEnt1.BooleanOperation(boolType, regionEnt2);
                        ent2.Dispose();
                    }
                }
                catch
                {
                    ed.WriteMessage("\n布尔操作失败！");
                    return false;
                }
                trans.Commit();
                return true;
            }
        }
    }
}

