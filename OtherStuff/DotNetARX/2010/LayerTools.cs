using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace DotNetARX
{
    /// <summary>
    /// 图层操作类
    /// </summary>
    public static class LayerTools
    {
        /// <summary>
        /// 创建新图层
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layerName">图层名</param>
        /// <returns>返回新建图层的ObjectId</returns>
        public static ObjectId AddLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            if (!lt.Has(layerName))//如果不存在名为layerName的图层，则新建一个图层
            {
                //定义一个新的层表记录
                LayerTableRecord ltr=new LayerTableRecord();
                ltr.Name = layerName;//设置图层名
                lt.UpgradeOpen();//切换层表的状态为写以添加新的图层
                //将层表记录的信息添加到层表中
                lt.Add(ltr);
                //把层表记录添加到事务处理中
                db.TransactionManager.AddNewlyCreatedDBObject(ltr, true);
                lt.DowngradeOpen();//为了安全，将层表的状态切换为读
            }
            return lt[layerName];//返回新添加的层表记录的ObjectId
        }

        /// <summary>
        /// 设置图层的颜色
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layerName">图层名</param>
        /// <param name="colorIndex">颜色索引</param>
        /// <returns>如果成功设置图层颜色，则返回true，否则返回false</returns>
        public static bool SetLayerColor(this Database db, string layerName, short colorIndex)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return false;
            ObjectId layerId=lt[layerName];//获取名为layerName的层表记录的Id
            //以写的方式打开名为layerName的层表记录
            LayerTableRecord ltr=(LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
            //设置图层的颜色
            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
            ltr.DowngradeOpen();//为了安全，将图层的状态切换为读
            return true;//设置图层颜色成功
        }

        /// <summary>
        /// 将指定的图层设置为当前层
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layerName">图层名</param>
        /// <returns>如果设置成功，则返回ture</returns>
        public static bool SetCurrentLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return false;
            //获取名为layerName的层表记录的Id
            ObjectId layerId=lt[layerName];
            //如果指定的图层为当前层，则返回
            if (db.Clayer == layerId) return false;
            db.Clayer = layerId;//指定当前层
            return true;//指定当前图层成功
        }

        /// <summary>
        /// 获取当前图形中所有的图层
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回所有的层表记录</returns>
        public static List<LayerTableRecord> GetAllLayers(this Database db)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //用于返回层表记录的列表
            List<LayerTableRecord> ltrs=new List<LayerTableRecord>();
            foreach (ObjectId id in lt)//遍历层表
            {
                //打开层表记录
                LayerTableRecord ltr=(LayerTableRecord)id.GetObject(OpenMode.ForRead);
                ltrs.Add(ltr);//添加到返回列表中
            }
            return ltrs;//返回所有的层表记录
        }

        /// <summary>
        /// 删除指定名称的图层
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="layerName">图层名</param>
        /// <returns>如果删除成功，则返回true，否则返回false</returns>
        public static bool DeleteLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果层名为0或Defpoints，则返回（这两个图层不能删除）
            if (layerName == "0" || layerName == "Defpoints") return false;
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return false;
            ObjectId layerId=lt[layerName];//获取名为layerName的层表记录的Id
            //如果要删除的图层为当前层，则返回（不能删除当前层）
            if (layerId == db.Clayer) return false;
            //打开名为layerName的层表记录
            LayerTableRecord ltr=(LayerTableRecord)layerId.GetObject(OpenMode.ForRead);
            //如果要删除的图层包含对象或依赖外部参照，则返回（不能删除这些层）
            lt.GenerateUsageData();
            if (ltr.IsUsed) return false;
            ltr.UpgradeOpen();//切换层表记录为写的状态
            ltr.Erase(true);//删除名为layerName的图层
            return true;//删除图层成功
        }

        /// <summary>
        /// 获取所有图层的ObjectId
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <returns>返回所有图层的ObjectId</returns>
        public static List<ObjectId> GetAllLayerObjectIds(this Database db)
        {
            //打开层表
            LayerTable lt=(LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //用于返回层表记录ObjectId的列表
            List<ObjectId> ltrs=new List<ObjectId>();
            //遍历层表
            foreach (ObjectId id in lt)
            {
                ltrs.Add(id);//添加到返回列表中
            }
            return ltrs;//返回所有的层表记录的ObjectId
        }

        /// <summary>
        /// 将图层解锁
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static void UnLockLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return;
            ObjectId layerId = lt[layerName];//获取名为layerName的层表记录的Id
            //以写的方式打开名为layerName的层表记录
            LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
            if (ltr != null)
            {
                if (ltr.IsLocked)
                {
                    ltr.IsLocked = false;
                }
            }
        }

        /// <summary>
        /// 将图层解冻
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static void UnFrozenLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return;
            ObjectId layerId = lt[layerName];//获取名为layerName的层表记录的Id
            //以写的方式打开名为layerName的层表记录
            LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
            if (ltr != null)
            {
                if (ltr.IsFrozen)
                {
                    ltr.IsFrozen = false;
                }
            }
        }

        /// <summary>
        /// 将图层打开
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static void UnOffLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return;
            ObjectId layerId = lt[layerName];//获取名为layerName的层表记录的Id
            //以写的方式打开名为layerName的层表记录
            LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
            if (ltr != null)
            {
                if (ltr.IsOff)
                {
                    ltr.IsOff = false;
                }
            }
        }

        /// <summary>
        /// 将图层设置为不可打印
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static void UnPrintLayer(this Database db, string layerName)
        {
            //打开层表
            LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            //如果不存在名为layerName的图层，则返回
            if (!lt.Has(layerName)) return;
            ObjectId layerId = lt[layerName];//获取名为layerName的层表记录的Id
            //以写的方式打开名为layerName的层表记录
            LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
            if (ltr != null)
            {
                if (ltr.IsPlottable)
                {
                    ltr.IsPlottable = false;
                }
            }
        }
    }
}
