﻿
#if a2024 || zcad
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 符号表记录扩展类
/// </summary>
public static class SymbolTableRecordEx
{
    #region 块表记录

    #region 克隆实体id

    /// <summary>
    /// 深度克隆id到块表记录
    /// <para>
    /// 0x01 此方法不允许是未添加数据库的图元,因此它是id<br/>
    /// 0x02 若为未添加数据库图元,则利用entity.Clone();同时不需要考虑动态块属性,可以使用entity.GetTransformedCopy
    /// </para>
    /// </summary>
    /// <param name="btr">块表记录
    /// <para>
    /// 克隆到当前块表记录,相当于原地克隆<br/>
    /// 克隆到目标块表记录内,相当于制作新块
    /// </para>
    /// </param>
    /// <param name="objIds">图元id集合,注意所有成员都要在同一个空间中</param>
    /// <param name="maoOut">返回克隆后的id词典</param>
    public static void DeepCloneEx(this BlockTableRecord btr, ObjectIdCollection objIds, IdMapping maoOut)
    {
        if (objIds is null || objIds.Count == 0)
            throw new System.ArgumentNullException(nameof(objIds));

        var db = objIds[0].Database;
        using (btr.ForWrite())
        {
            try
            {
                db.DeepCloneObjects(objIds, btr.ObjectId, maoOut, false);

                // 不在此提取,为了此函数被高频调用
                // 获取克隆键值对(旧块名,新块名)
                // foreach (ObjectId item in blockIds)
                //     result.Add(mapping[item].Value);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// 深度克隆id到块表记录
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="objIds">图元id集合</param>
    /// <returns>id词典</returns>
    public static IdMapping DeepCloneEx(this BlockTableRecord btr, ObjectIdCollection objIds)
    {
        if (objIds is null || objIds.Count == 0)
            throw new System.ArgumentNullException(nameof(objIds));

        var db = objIds[0].Database;
        IdMapping mapOut = new();
        using (btr.ForWrite())
        {
            try
            {
                db.DeepCloneObjects(objIds, btr.ObjectId, mapOut, false);

                // 不在此提取,为了此函数被高频调用
                // 获取克隆键值对(旧块名,新块名)
                // foreach (ObjectId item in blockIds)
                //     result.Add(mapping[item].Value);
            }
            catch
            {
                // ignored
            }
        }

        return mapOut;
    }

    #endregion

    #region 添加实体

    /// <summary>
    /// 添加实体对象
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="entity">实体</param>
    /// <returns>对象 id</returns>
    public static ObjectId AddEntity(this BlockTableRecord btr, Entity entity)
    {
        ObjectId id;
        var tr = DBTrans.GetTopTransaction(btr.Database);
        using (btr.ForWrite())
        {
            id = btr.AppendEntity(entity);
            tr.AddNewlyCreatedDBObject(entity, true);
        }

        return id;
    }

    /// <summary>
    /// 添加实体集合
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="ents">实体集合</param>
    /// <returns>对象 id 列表</returns>
    public static IEnumerable<ObjectId> AddEntity(this BlockTableRecord btr, IEnumerable<Entity> ents)
    {
        var tr = DBTrans.GetTopTransaction(btr.Database);
        using (btr.ForWrite())
        {
            return ents.Select(ent =>
            {
                var id = btr.AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);
                return id;
            }).ToList();
        }
    }

    /// <summary>
    /// 添加多个实体
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <param name="ents">实体集合</param>
    /// <returns>对象 id 列表</returns>
    public static IEnumerable<ObjectId> AddEntity(this BlockTableRecord btr, params Entity[] ents)
    {
        return btr.AddEntity(ents.ToList());
    }

    #endregion

    #region 获取实体/实体id

    /// <summary>
    /// 获取块表记录内的指定类型的实体
    /// (此处不会检查id.IsOk())
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="btr">块表记录</param>
    /// <param name="openMode">打开模式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>实体集合</returns>
    public static IEnumerable<T> GetEntities<T>(this BlockTableRecord btr,
        OpenMode openMode = OpenMode.ForRead,
        bool openErased = false,
        bool openLockedLayer = false) where T : Entity
    {
        var rxc = RXObject.GetClass(typeof(T));
        return
            btr
                .Cast<ObjectId>()
                .Where(id => id.ObjectClass.IsDerivedFrom(rxc))
                .Select(id => id.GetObject<T>(openMode, openErased, openLockedLayer))
                .OfType<T>();
    }

    /// <summary>
    /// 按类型获取实体Id
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="btr">块表记录</param>
    /// <returns>实体Id集合</returns>
    public static IEnumerable<ObjectId> GetObjectIds<T>(this BlockTableRecord btr) where T : Entity
    {
        var dxfName = RXObject.GetClass(typeof(T)).DxfName;
        return btr.Cast<ObjectId>()
            .Where(id => id.ObjectClass.DxfName == dxfName);
    }

    /// <summary>
    /// 按类型获取实体Id的分组
    /// </summary>
    /// <param name="btr">块表记录</param>
    /// <returns>实体Id分组</returns>
    public static IEnumerable<IGrouping<string, ObjectId>> GetObjectIds(this BlockTableRecord btr)
    {
        return btr.Cast<ObjectId>()
            .GroupBy(id => id.ObjectClass.DxfName);
    }


    /// <summary>
    /// 获取绘制顺序表
    /// </summary>
    /// <param name="btr">块表</param>
    /// <param name="openMode">开启方式</param>
    /// <param name="openErased">是否打开已删除对象,默认为不打开</param>
    /// <param name="openLockedLayer">是否打开锁定图层对象,默认为不打开</param>
    /// <returns>绘制顺序表</returns>
    public static DrawOrderTable GetDrawOrderTable(this BlockTableRecord btr,
        OpenMode openMode = OpenMode.ForRead,
        bool openErased = false,
        bool openLockedLayer = false)
    {
        var tr = DBTrans.GetTopTransaction(btr.Database);
        return (DrawOrderTable)tr.GetObject(btr.DrawOrderTableId, openMode, openErased, openLockedLayer);
    }

    #endregion

    #region 插入块参照

    /// <summary>
    /// 插入块参照
    /// </summary>
    /// <param name="blockTableRecord">块表记录</param>
    /// <param name="position">插入点</param>
    /// <param name="blockName">块名</param>
    /// <param name="scale">块插入比例，默认为1</param>
    /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
    /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
    /// <returns>块参照对象id</returns>
    public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord, Point3d position,
        string blockName,
        Scale3d scale = default,
        double rotation = default,
        Dictionary<string, string>? atts = null)
    {
        var tr = DBTrans.GetTop(blockTableRecord.Database);
        if (!tr.BlockTable.Has(blockName))
        {
            tr.Editor?.WriteMessage($"\n不存在名字为{blockName}的块定义。");
            return ObjectId.Null;
        }

        return blockTableRecord.InsertBlock(position, tr.BlockTable[blockName], scale, rotation, atts);
    }

    /// <summary>
    /// 插入块参照
    /// </summary>
    /// <param name="blockTableRecord">块表记录</param>
    /// <param name="position">插入点</param>
    /// <param name="blockId">块定义id</param>
    /// <param name="scale">块插入比例，默认为1</param>
    /// <param name="rotation">块插入旋转角(弧度)，默认为0</param>
    /// <param name="atts">属性字典{Tag,Value}，默认为null</param>
    /// <returns>块参照对象id</returns>
    public static ObjectId InsertBlock(this BlockTableRecord blockTableRecord,
        Point3d position,
        ObjectId blockId,
        Scale3d scale = default,
        double rotation = default,
        Dictionary<string, string>? atts = null)
    {
        //trans ??= DBTrans.Top.Transaction;
        var tr = DBTrans.GetTop(blockTableRecord.Database);

        if (!tr.BlockTable.Has(blockId))
        {
            tr.Editor?.WriteMessage($"\n不存在块定义。");
            return ObjectId.Null;
        }


        using var blockRef = new BlockReference(position, blockId);
        blockRef.ScaleFactors = scale;
        blockRef.Rotation = rotation;
        var objectId = blockTableRecord.AddEntity(blockRef);
        // 检查块的注释性
        using var ocm = blockTableRecord.Database.ObjectContextManager;
        using var occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
        if (blockRef.Annotative == AnnotativeStates.True)
            blockRef.AddContext(occ.CurrentContext);

        var btr = tr.GetObject<BlockTableRecord>(blockRef.BlockTableRecord)!;

        if (!btr.HasAttributeDefinitions) 
            return objectId;

        var attdefs = btr.GetEntities<AttributeDefinition>();
        foreach (var attdef in attdefs)
        {
            using AttributeReference attref = new();
            attref.SetDatabaseDefaults();
            attref.SetAttributeFromBlock(attdef, blockRef.BlockTransform);
            attref.Position = attdef.Position.TransformBy(blockRef.BlockTransform);
            if (atts is not null && atts.TryGetValue(attdef.Tag, out var str))
            {
                attref.TextString = str;
            }

            if (blockRef.Annotative == AnnotativeStates.True)
                attref.AddContext(occ.CurrentContext);

            blockRef.AttributeCollection.AppendAttribute(attref);
            tr.Transaction.AddNewlyCreatedDBObject(attref, true);
            attref.AdjustAlignment(tr.Database);
        }

        return objectId;
    }

    #endregion

    #endregion

    #region 遍历

#line hidden // 调试的时候跳过它
    /// <summary>
    /// 遍历符号表记录,执行委托
    /// </summary>
    /// <param name="record">符号表记录</param>
    /// <param name="task">要运行的委托</param>
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId> task)
        where TRecord : SymbolTableRecord, IEnumerable
    {
        foreach (ObjectId id in record)
            task.Invoke(id);
    }

    /// <summary>
    /// 遍历符号表记录,执行委托(允许循环中断)
    /// </summary>
    /// <param name="record">符号表记录</param>
    /// <param name="task">要执行的委托</param>
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId, LoopState> task)
        where TRecord : SymbolTableRecord, IEnumerable
    {
        LoopState state = new(); /*这种方式比Action改Func更友好*/
        foreach (ObjectId id in record)
        {
            task.Invoke(id, state);
            if (!state.IsRun)
                break;
        }
    }

    /// <summary>
    /// 遍历符号表记录,执行委托(允许循环中断,输出索引值)
    /// </summary>
    /// <param name="record">符号表记录</param>
    /// <param name="task">要执行的委托</param>
    [DebuggerStepThrough]
    public static void ForEach<TRecord>(this TRecord record, Action<ObjectId, LoopState, int> task)
        where TRecord : SymbolTableRecord, IEnumerable
    {
        //if (task == null)
        //    throw new ArgumentNullException(nameof(task));
        ArgumentNullException.ThrowIfNull(task);
        var i = 0;
        LoopState state = new(); /*这种方式比Action改Func更友好*/
        foreach (ObjectId id in record)
        {
            task.Invoke(id, state, i);
            if (!state.IsRun)
                break;
            i++;
        }
    }
#line default

    #endregion
}