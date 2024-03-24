﻿namespace IFoxCAD.Cad;

/// <summary>
/// 块参照扩展类
/// </summary>
public static class BlockReferenceEx
{
    #region 裁剪块参照

    private const string filterDictName = "ACAD_FILTER";
    private const string spatialName = "SPATIAL";

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="brf">块参照</param>
    /// <param name="pt3ds">裁剪多边形点表</param>
    public static void XClip(this BlockReference brf, IEnumerable<Point3d> pt3ds)
    {
        var mat = brf.BlockTransform.Inverse();
        var pts =
            pt3ds
                .Select(p => p.TransformBy(mat).Point2d())
                .ToCollection();

        SpatialFilterDefinition sfd = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
        using SpatialFilter sf = new();
        sf.Definition = sfd;
        var dict = brf.GetXDictionary().GetSubDictionary(true, [filterDictName])!;
        dict.SetData(spatialName, sf);
    }

    /// <summary>
    /// 裁剪块参照
    /// </summary>
    /// <param name="brf">块参照</param>
    /// <param name="pt1">第一角点</param>
    /// <param name="pt2">第二角点</param>
    public static void XClip(this BlockReference brf, Point3d pt1, Point3d pt2)
    {
        var mat = brf.BlockTransform.Inverse();
        pt1 = pt1.TransformBy(mat);
        pt2 = pt2.TransformBy(mat);

        Point2dCollection pts =
        [
            new Point2d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y)),
            new Point2d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y))
        ];

        using SpatialFilter sf = new();
        sf.Definition = new(pts, Vector3d.ZAxis, 0.0, 0.0, 0.0, true);
        var dict = brf.GetXDictionary()
            .GetSubDictionary(true, [filterDictName])!;
        dict.SetData(spatialName, sf);
#if !acad
        pts.Dispose();
#endif
    }

    #endregion

    #region 属性

    /// <summary>
    /// 更新动态块属性值
    /// </summary>
    public static void ChangeBlockProperty<T>(this BlockReference blockReference,
        Dictionary<string, T> propertyNameValues)
    {
        using (blockReference.ForWrite())
        {
            foreach (DynamicBlockReferenceProperty item in blockReference.DynamicBlockReferencePropertyCollection)
            {
                // TODO 这里太烂了，应该判断类型
                if (propertyNameValues.TryGetValue(item.PropertyName, out var value))
                {
                    item.Value = value;
                }
            }
        }
    }

    /// <summary>
    /// 更新普通块的属性值
    /// </summary>
    public static void ChangeBlockAttribute(this BlockReference blockReference, Dictionary<string, string> propertyNameValues)
    {
        var tr = DBTrans.GetTopTransaction(blockReference.Database);
        foreach (var item in blockReference.AttributeCollection)
        {
            AttributeReference att;
            if (item is ObjectId id)
            {
                // 通常情况下返回的都是 ObjectId
                att = (AttributeReference)tr.GetObject(id);
            }
            else
            {
                // 某些情况下，比如你exploded炸开块后的子块块参照是没有在数据库里的，这时候返回的结果就是 AttributeReference
                att = (AttributeReference)item;
            }

            using (att.ForWrite())
            {
                if (propertyNameValues.TryGetValue(att.Tag, out var value))
                {
                    att.TextString = value;
                }
            }
        }
    }

    /// <summary>
    /// 获取嵌套块的位置(wcs)
    /// </summary>
    /// <param name="parentBlockRef">父块</param>
    /// <param name="nestedBlockName">子块名</param>
    /// <returns>子块的位置</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Point3d? GetNestedBlockPosition(this BlockReference parentBlockRef, string nestedBlockName)
    {
        var tr = DBTrans.GetTopTransaction(parentBlockRef.Database);

        var btr = tr.GetObject<BlockTableRecord>(parentBlockRef.BlockTableRecord);
        if (btr == null) return null;
        foreach (var id in btr)
        {
            if (id.ObjectClass.Name == "AcDbBlockReference")
            {
                var nestedBlockRef = tr.GetObject<BlockReference>(id);
                if (nestedBlockRef?.Name == nestedBlockName)
                {
                    return nestedBlockRef.Position.TransformBy(parentBlockRef.BlockTransform);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 获取普通块参照的属性集合
    /// </summary>
    /// <param name="owner">普通块参照</param>
    /// <returns>属性集合</returns>
    public static IEnumerable<AttributeReference> GetAttributes(this BlockReference owner)
    {
        if (owner.Database != null)
        {
            var trans = DBTrans.GetTopTransaction(owner.Database);
            foreach (ObjectId id in owner.AttributeCollection)
                yield return (AttributeReference)trans.GetObject(id);
        }
        else
        {
            foreach (AttributeReference att in owner.AttributeCollection)
                yield return att;
        }
    }

    #endregion

    /// <summary>
    /// 获取块的有效名字
    /// </summary>
    /// <param name="blk">块参照</param>
    /// <returns>名字</returns>
    public static string GetBlockName(this BlockReference blk)
    {
        ArgumentNullEx.ThrowIfNull(blk);
        if (blk.IsDynamicBlock)
        {
            var btrId = blk.DynamicBlockTableRecord;
            var tr = btrId.Database.TransactionManager.TopTransaction;
            ArgumentNullEx.ThrowIfNull(tr);
            var btr = (BlockTableRecord)tr.GetObject(btrId);
            return btr.Name;
        }

        return blk.Name;
    }

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId> action)
    {
        ArgumentNullEx.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState> action)
    {
        ArgumentNullEx.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void ForEach(this BlockReference brf, Action<ObjectId, LoopState, int> action)
    {
        ArgumentNullEx.ThrowIfNull(action);
        var tr = DBTrans.GetTopTransaction(brf.Database);
        if (tr.GetObject(brf.BlockTableRecord) is BlockTableRecord btr)
        {
            btr.ForEach(action);
        }
    }

    /// <summary>
    /// 遍历嵌套块中块图元
    /// </summary>
    /// <param name="blockReference">块参照</param>
    /// <param name="action">委托</param>
    /// <param name="tr">事务</param>
    public static void NestedForEach(this Entity blockReference, Action<Entity, Matrix3d> action, DBTrans? tr = null)
    {
        tr ??= DBTrans.GetTop(blockReference.IsNewObject ? Env.Database : blockReference.Database);
        var queue = new Queue<(Entity, Matrix3d)>();
        queue.Enqueue((blockReference, Matrix3d.Identity));
        while (queue.Any())
        {
            var (ent, mt) = queue.Dequeue();
            action?.Invoke(ent, mt);
            if (ent is BlockReference brfTemp)
            {
                var mtNext = mt * brfTemp.BlockTransform;
                tr.BlockTable.Change(brfTemp.BlockTableRecord, btr =>
                {
                    foreach (var id in btr)
                    {
                        if (tr.GetObject(id) is Entity entNext)
                        {
                            queue.Enqueue((entNext, mtNext));
                        }
                    }
                });
            }
        }
    }
}