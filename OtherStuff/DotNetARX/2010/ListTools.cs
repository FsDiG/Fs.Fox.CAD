using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace DotNetARX
{
    /// <summary>
    /// TypedValue列表类，简化选择集过滤器的构造
    /// </summary>
    public class TypedValueList : List<TypedValue>
    {
        /// <summary>
        /// 接受可变参数的构造函数
        /// </summary>
        /// <param name="args">TypedValue对象</param>
        public TypedValueList(params TypedValue[] args)
        {
            AddRange(args);
        }

        /// <summary>
        /// 添加DXF组码及对应的类型
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        /// <param name="value">类型</param>
        public void Add(int typecode, object value)
        {
            base.Add(new TypedValue(typecode, value));
        }

        /// <summary>
        /// 添加DXF组码
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        public void Add(int typecode)
        {
            base.Add(new TypedValue(typecode));
        }

        /// <summary>
        /// 添加DXF组码及对应的类型
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        /// <param name="value">类型</param>
        public void Add(DxfCode typecode, object value)
        {
            base.Add(new TypedValue((int)typecode, value));
        }

        /// <summary>
        /// 添加DXF组码
        /// </summary>
        /// <param name="typecode">DXF组码</param>
        public void Add(DxfCode typecode)
        {
            base.Add(new TypedValue((int)typecode));
        }

        /// <summary>
        /// 添加图元类型,DXF组码缺省为0
        /// </summary>
        /// <param name="entityType">图元类型</param>
        public void Add(Type entityType)
        {
            base.Add(new TypedValue(0, RXClass.GetClass(entityType).DxfName));
        }

        /// <summary>
        /// TypedValueList隐式转换为SelectionFilter
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的SelectionFilter类对象</returns>
        public static implicit operator SelectionFilter(TypedValueList src)
        {
            return src != null ? new SelectionFilter(src) : null;
        }

        /// <summary>
        /// TypedValueList隐式转换为ResultBuffer
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的ResultBuffer对象</returns>
        public static implicit operator ResultBuffer(TypedValueList src)
        {
            return src != null ? new ResultBuffer(src) : null;
        }

        /// <summary>
        /// TypedValueList隐式转换为TypedValue数组
        /// </summary>
        /// <param name="src">要转换的TypedValueList对象</param>
        /// <returns>返回对应的TypedValue数组</returns>
        public static implicit operator TypedValue[](TypedValueList src)
        {
            return src != null ? src.ToArray() : null;
        }

        /// <summary>
        /// TypedValue数组隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的TypedValue数组</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(TypedValue[] src)
        {
            return src != null ? new TypedValueList(src) : null;
        }

        /// <summary>
        /// SelectionFilter隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的SelectionFilter</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(SelectionFilter src)
        {
            return src != null ? new TypedValueList(src.GetFilter()) : null;
        }

        /// <summary>
        /// ResultBuffer隐式转换为TypedValueList
        /// </summary>
        /// <param name="src">要转换的ResultBuffer</param>
        /// <returns>返回对应的TypedValueList</returns>
        public static implicit operator TypedValueList(ResultBuffer src)
        {
            return src != null ? new TypedValueList(src.AsArray()) : null;
        }

    }

    /// <summary>
    /// Point3d列表类
    /// </summary>
    public class Point3dList : List<Point3d>
    {
        /// <summary>
        /// 接受可变参数的构造函数
        /// </summary>
        /// <param name="args">Point3d类对象</param>
        public Point3dList(params Point3d[] args)
        {
            AddRange(args);
        }

        /// <summary>
        /// Point3dList隐式转换为Point3d数组
        /// </summary>
        /// <param name="src">要转换的Point3dList对象</param>
        /// <returns>返回对应的Point3d数组</returns>
        public static implicit operator Point3d[](Point3dList src)
        {
            return src != null ? src.ToArray() : null;
        }

        /// <summary>
        /// Point3dList隐式转换为Point3dCollection
        /// </summary>
        /// <param name="src">要转换的Point3dList对象</param>
        /// <returns>返回对应的Point3dCollection</returns>
        public static implicit operator Point3dCollection(Point3dList src)
        {
            return src != null ? new Point3dCollection(src) : null;
        }

        /// <summary>
        /// Point3d数组隐式转换为Point3dList
        /// </summary>
        /// <param name="src">要转换的Point3d数组</param>
        /// <returns>返回对应的Point3dList</returns>
        public static implicit operator Point3dList(Point3d[] src)
        {
            return src != null ? new Point3dList(src) : null;
        }

        /// <summary>
        /// Point3dCollection隐式转换为Point3dList
        /// </summary>
        /// <param name="src">要转换的Point3dCollection</param>
        /// <returns>返回对应的Point3dList</returns>
        public static implicit operator Point3dList(Point3dCollection src)
        {
            if (src != null)
            {
                Point3d[] ids = new Point3d[src.Count];
                src.CopyTo(ids, 0);
                return new Point3dList(ids);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// ObjectId列表
    /// </summary>
    public class ObjectIdList : List<ObjectId>
    {
        /// <summary>
        /// 接受可变参数的构造函数
        /// </summary>
        /// <param name="args">ObjectId对象</param>
        public ObjectIdList(params ObjectId[] args)
        {
            AddRange(args);
        }

        /// <summary>
        /// ObjectIdList隐式转换为ObjectId数组
        /// </summary>
        /// <param name="src">要转换的ObjectIdList对象</param>
        /// <returns>返回对应的ObjectId数组</returns>
        public static implicit operator ObjectId[](ObjectIdList src)
        {
            return src != null ? src.ToArray() : null;
        }

        /// <summary>
        /// ObjectIdList隐式转换为ObjectIdCollection
        /// </summary>
        /// <param name="src">要转换的ObjectIdList对象</param>
        /// <returns>返回对应的ObjectIdCollection</returns>
        public static implicit operator ObjectIdCollection(ObjectIdList src)
        {
            return src != null ? new ObjectIdCollection(src) : null;
        }

        /// <summary>
        /// ObjectId数组隐式转换为ObjectIdList
        /// </summary>
        /// <param name="src">要转换的ObjectId数组</param>
        /// <returns>返回对应的ObjectIdList</returns>
        public static implicit operator ObjectIdList(ObjectId[] src)
        {
            return src != null ? new ObjectIdList(src) : null;
        }

        /// <summary>
        /// ObjectIdCollection隐式转换为ObjectIdList
        /// </summary>
        /// <param name="src">要转换的ObjectIdCollection</param>
        /// <returns>返回对应的ObjectIdList</returns>
        public static implicit operator ObjectIdList(ObjectIdCollection src)
        {
            if (src != null)
            {
                ObjectId[] ids = new ObjectId[src.Count];
                src.CopyTo(ids, 0);
                return new ObjectIdList(ids);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Entity列表
    /// </summary>
    public class EntityList : List<Entity>
    {
        /// <summary>
        /// 接受可变参数的构造函数
        /// </summary>
        /// <param name="args">实体对象</param>
        public EntityList(params Entity[] args)
        {
            AddRange(args);
        }

        /// <summary>
        /// EntityList隐式转换为Entity数组
        /// </summary>
        /// <param name="src">要转换的EntityList</param>
        /// <returns>返回对应的Entity数组</returns>
        public static implicit operator Entity[](EntityList src)
        {
            return src != null ? src.ToArray() : null;
        }

        /// <summary>
        /// Entity数组隐式转换为EntityList
        /// </summary>
        /// <param name="src">要转换的Entity数组</param>
        /// <returns>返回对应的EntityList</returns>
        public static implicit operator EntityList(Entity[] src)
        {
            return src != null ? new EntityList(src) : null;
        }

    }
}
