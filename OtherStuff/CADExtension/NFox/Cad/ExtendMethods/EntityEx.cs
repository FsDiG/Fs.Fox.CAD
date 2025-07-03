using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace NFox.Cad
{
    /// <summary>
    /// 实体图元扩展类
    /// </summary>
    public static class EntityEx
    {
        #region Vertex

        /// <summary>
        /// 获取复合实体的子实体集合
        /// </summary>
        /// <typeparam name="T">复合实体类型(PolyLine2d/3d、PolyfaceMesh......)</typeparam>
        /// <param name="ent">复合实体</param>
        /// <param name="tr">事务实例</param>
        /// <returns>子实体迭代器</returns>
        public static IEnumerable<Vertex> GetVertexs<T>(this T ent, Transaction tr) where T : Entity, IEnumerable
        {
            foreach (ObjectId id in ent)
            {
                yield return (Vertex)tr.GetObject(id, OpenMode.ForRead);
            }
        }

        /// <summary>
        /// 获取复合实体的子实体集合
        /// </summary>
        /// <typeparam name="T">复合实体类型(PolyLine2d/3d、PolyfaceMesh......)</typeparam>
        /// <param name="ent">复合实体</param>
        /// <param name="tr">事务实例</param>
        /// <param name="mode">打开模式(读/写)</param>
        /// <returns>子实体迭代器</returns>
        public static IEnumerable<Vertex> GetVertexs<T>(this T ent, Transaction tr, OpenMode mode) where T : Entity, IEnumerable
        {
            foreach (ObjectId id in ent)
            {
                yield return (Vertex)tr.GetObject(id, mode);
            }
        }

        /// <summary>
        /// 获取二维多段线的端点坐标
        /// </summary>
        /// <param name="pl2d">二维多段线</param>
        /// <param name="tr">事务</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline2d pl2d, Transaction tr)
        {
            foreach (ObjectId id in pl2d)
            {
                yield return ((Vertex2d)tr.GetObject(id, OpenMode.ForRead)).Position;
            }
        }

        /// <summary>
        /// 获取三维多段线的端点坐标
        /// </summary>
        /// <param name="pl3d">三维多段线</param>
        /// <param name="tr">事务</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline3d pl3d, Transaction tr)
        {
            foreach (ObjectId id in pl3d)
            {
                yield return ((PolylineVertex3d)tr.GetObject(id, OpenMode.ForRead)).Position;
            }
        }

        /// <summary>
        /// 获取多段线的端点坐标
        /// </summary>
        /// <param name="pl">多段线</param>
        /// <returns>端点坐标集合</returns>
        public static IEnumerable<Point3d> GetPoints(this Polyline pl)
        {
            return
                Enumerable
                .Range(0, pl.NumberOfVertices)
                .Select(i => pl.GetPoint3dAt(i));
        }

        #endregion Vertex

        #region GetGroup

        /// <summary>
        /// 获取图元的分组
        /// </summary>
        /// <param name="ent">图元</param>
        /// <param name="tr">事务</param>
        /// <returns>组的集合</returns>
        public static IEnumerable<Group> GetGroups(this Entity ent, Transaction tr)
        {
            return
                ent
                .GetPersistentReactorIds()
                .Cast<ObjectId>()
                .GetObject(tr)
                .OfType<Group>();
        }

        /// <summary>
        /// 获取图元的分组ID
        /// </summary>
        /// <param name="ent">图元</param>
        /// <returns>组id集合</returns>
        public static IEnumerable<ObjectId> GetGroupIds(this Entity ent)
        {
            return
                ent
                .GetPersistentReactorIds()
                .Cast<ObjectId>();
        }

        #endregion GetGroup

        #region TransformBy

        /// <summary>
        /// 移动实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="from">基点</param>
        /// <param name="to">目标点</param>
        public static void Move(this Entity ent, Point3d from, Point3d to)
        {
            ent.TransformBy(Matrix3d.Displacement(to - from));
        }

        /// <summary>
        /// 缩放实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">缩放几点坐标</param>
        /// <param name="scaleValue">缩放比例</param>
        public static void Scale(this Entity ent, Point3d center, double scaleValue)
        {
            ent.TransformBy(Matrix3d.Scaling(scaleValue, center));
        }

        /// <summary>
        /// 旋转实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angle">转角</param>
        /// <param name="normal">旋转平面的法向矢量</param>
        public static void Rotation(this Entity ent, Point3d center, double angle, Vector3d normal)
        {
            ent.TransformBy(Matrix3d.Rotation(angle, normal, center));
        }

        /// <summary>
        /// 在XY平面内旋转实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">旋转中心</param>
        /// <param name="angle">转角</param>
        public static void Rotation(this Entity ent, Point3d center, double angle)
        {
            ent.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis.TransformBy(ent.Ecs), center));
        }

        /// <summary>
        /// 按对称轴镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="startPoint">对称轴起点</param>
        /// <param name="endPoint">对称轴终点</param>
        public static void Mirror(this Entity ent, Point3d startPoint, Point3d endPoint)
        {
            ent.TransformBy(Matrix3d.Mirroring(new Line3d(startPoint, endPoint)));
        }

        /// <summary>
        /// 按对称面镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="plane">对称平面</param>
        public static void Mirror(this Entity ent, Plane plane)
        {
            ent.TransformBy(Matrix3d.Mirroring(plane));
        }

        /// <summary>
        /// 按对称点镜像实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="basePoint">对称点</param>
        public static void Mirror(this Entity ent, Point3d basePoint)
        {
            ent.TransformBy(Matrix3d.Mirroring(basePoint));
        }

        /// <summary>
        /// 更正单行文字的镜像属性
        /// </summary>
        /// <param name="txt">单行文字</param>
        public static void ValidateMirror(this DBText txt)
        {
            if (!txt.Database.Mirrtext)
            {
                txt.IsMirroredInX = false;
                txt.IsMirroredInY = false;
            }
        }

        #endregion TransformBy

        /// <summary>
        /// 获取实体集合的范围
        /// </summary>
        /// <param name="ents">实体迭代器</param>
        /// <returns>实体集合的范围</returns>
        public static Extents3d GetExtents(this IEnumerable<Entity> ents)
        {
            var it = ents.GetEnumerator();
            var ext = it.Current.GeometricExtents;
            while (it.MoveNext())
                ext.AddExtents(it.Current.GeometricExtents);
            return ext;
        }

        /// <summary>
        /// 炸散多行文字
        /// </summary>
        /// <typeparam name="T">存储多行文字炸散之后的对象的类型</typeparam>
        /// <param name="mt">多行文字</param>
        /// <param name="obj">存储对象变量</param>
        /// <param name="mTextFragmentCallback">回调函数，用于处理炸散之后的对象
        /// <para>MTextFragment -- 多行文字炸散后的对象</para>
        /// <para>MTextFragmentCallbackStatus -- 回调函数处理的结果</para>
        /// </param>
        public static void ExplodeFragments<T>(this MText mt, T obj, Func<MTextFragment, T, MTextFragmentCallbackStatus> mTextFragmentCallback)
        {
            mt.ExplodeFragments(
                (f, o) => mTextFragmentCallback(f, (T)o),
                obj);
        }

        /// <summary>
        /// 获取多行文字的无格式文本
        /// </summary>
        /// <param name="mt">多行文字</param>
        /// <returns>文本</returns>
        public static string GetUnFormatString(this MText mt)
        {
            List<string> strs = new List<string>();
            mt.ExplodeFragments(
                strs,
                (f, o) =>
                {
                    o.Add(f.Text);
                    return MTextFragmentCallbackStatus.Continue;
                });
            return string.Join("", strs.ToArray());
        }
    }
}