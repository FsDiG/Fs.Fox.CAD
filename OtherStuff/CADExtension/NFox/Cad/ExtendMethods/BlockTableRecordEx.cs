using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace NFox.Cad
{
    /// <summary>
    /// 块表扩展类
    /// </summary>
    public static class BlockTableRecordEx
    {
        /// <summary>
        /// 按类型获取实体Id,AutoCad2010以上版本支持
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <returns>实体Id集合</returns>
        public static IEnumerable<ObjectId> GetObjectIds<T>(this BlockTableRecord btr) where T : Entity
        {
            string dxfName = RXClass.GetClass(typeof(T)).DxfName;
            return
                btr
                .Cast<ObjectId>()
                .Where(id => id.ObjectClass.DxfName == dxfName);
        }

        /// <summary>
        /// 按类型获取实体Id的分组
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <returns>实体Id分组</returns>
        public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIds(this BlockTableRecord btr)
        {
            return
                btr
                .Cast<ObjectId>()
                .GroupBy(id => id.ObjectClass.DxfName);
        }

        /// <summary>
        /// 按类型获取实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>实体集合</returns>
        public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr, Transaction tr, OpenMode mode) where T : Entity
        {
            return
                btr
                .Cast<ObjectId>()
                .Select(id => tr.GetObject(id, mode))
                .OfType<T>();
        }

        /// <summary>
        /// 按类型获取实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <returns>实体集合</returns>
        public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr, Transaction tr) where T : Entity
        {
            return GetEntities<T>(btr, tr, OpenMode.ForRead);
        }

        /// <summary>
        /// 获取 Entity 类型实体
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="mode">打开模式</param>
        /// <returns>实体集合</returns>
        public static IEnumerable<Entity> GetEntities(this BlockTableRecord btr, Transaction tr, OpenMode mode)
        {
            return
                btr
                .Cast<ObjectId>()
                .Select(id => tr.GetObject(id, mode))
                .Cast<Entity>();
        }

        /// <summary>
        /// 获取 Entity 类型实体
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <returns>实体集合</returns>
        public static IEnumerable<Entity> GetEntities(this BlockTableRecord btr, Transaction tr)
        {
            return GetEntities(btr, tr, OpenMode.ForRead);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="entity">实体对象</param>
        /// <returns>实体ID</returns>
        public static ObjectId AddEntity(this BlockTableRecord btr, Transaction tr, Entity entity)
        {
            using (btr.UpgradeOpenAndRun())
            {
                ObjectId id = btr.AppendEntity(entity);
                tr.AddNewlyCreatedDBObject(entity, true);
                return id;
            }
        }

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="ents">实体集合</param>
        /// <returns>对象 id 列表</returns>
        public static List<ObjectId> AddEntity<T>(this BlockTableRecord btr, Transaction tr, IEnumerable<T> ents) where T : Entity
        {
            using (btr.UpgradeOpenAndRun())
            {
                return
                    ents
                    .Select(
                        ent =>
                        {
                            ObjectId id = btr.AppendEntity(ent);
                            tr.AddNewlyCreatedDBObject(ent, true);
                            return id;
                        })
                    .ToList();
            }
        }

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="btr">块表记录</param>
        /// <param name="tr">事务</param>
        /// <param name="objs">实体集合</param>
        /// <returns>对象 id 列表</returns>
        public static List<ObjectId> AddEntity(this BlockTableRecord btr, Transaction tr, DBObjectCollection objs)
        {
            return AddEntity(btr, tr, objs.Cast<Entity>());
        }

        /// <summary>
        /// 获取绘制顺序表
        /// </summary>
        /// <param name="btr">块表</param>
        /// <param name="tr">事务</param>
        /// <returns>绘制顺序表</returns>
        public static DrawOrderTable GetDrawOrderTable(this BlockTableRecord btr, Transaction tr)
        {
            return tr.GetObject(btr.DrawOrderTableId, OpenMode.ForRead) as DrawOrderTable;
        }
    }
}