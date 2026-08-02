// ReSharper disable ForCanBeConvertedToForeach

namespace Fs.Fox.Cad;

/// <summary>
/// 参照工厂类
/// </summary>
/// <param name="tr"></param>
/// <param name="xrefNames">要处理的参照名称,<see langword="null"/>就处理所有</param>
public class XrefFactory(DBTrans tr, HashSet<string>? xrefNames = null) : IXrefBindModes
{
    #region 私有字段

    private readonly DBTrans _tr = tr;

    /// <summary>
    /// 要处理的参照名称,<see langword="null"/>就处理所有
    /// </summary>
    private readonly HashSet<string>? _xrefNames = xrefNames;

    #endregion

    #region 公开字段

    /// <summary>
    /// 解析外部参照:线性引擎<br/>
    /// 默认<see langword="false"/><br/>
    /// <see langword="true"/>时会在cad命令历史打印一些AEC信息,并导致绑定慢一点...具体作用不详<br/>
    /// </summary>
    public readonly bool UseThreadEngine = false;

    /// <summary>
    /// 解析外部参照:仅处理 Unresolved_未融入(未解析)的参照<br/>
    /// 默认<see langword="true"/>
    /// </summary>
    public readonly bool DoNewOnly = true;

    /// <summary>
    /// 解析外部参照:包含僵尸参照
    /// </summary>
    public readonly bool IncludeGhosts = true;


    /// <summary>
    /// 绑定模式和双美元符号相关(与cad保持相同地默认)<br/>
    /// <see langword="false"/>为绑定模式,产生双美元;
    /// <see langword="true"/>为插入模式,块重名会以本图覆盖;
    /// </summary>
    public readonly bool BindOrInsert = false;

    /// <summary>
    /// bind时候是否拆离参照<br/>
    /// 默认<see langword="true"/>:学官方的绑定后自动拆离
    /// </summary>
    public readonly bool AutoDetach = true;

    /// <summary>
    /// bind时候是否删除被卸载的嵌套参照<br/>
    /// 默认<see langword="true"/>
    /// </summary>
    public readonly bool EraseNested = true;

    /// <summary>
    /// bind时候控制绑定的符号表:请保持默认<br/>
    /// 目前仅推荐用于<see cref="SymModes.LayerTable"/>项<br/>
    /// 其他项有异常:<see langword="eWasOpenForNotify"/><br/>
    /// </summary>
    public readonly SymModes SymModesBind = SymModes.LayerTable;

    #endregion


    #region 重写

    /// <summary>
    /// 绑定
    /// </summary>
    public void Bind()
    {
        // 此功能有绑定出错的问题
        // db.BindXrefs(xrefIds, true);

        // 绑定后会自动拆离
        // 此功能修补了上面缺失
        DoubleBind();
    }

    /// <summary>
    /// 分离
    /// </summary>
    public void Detach()
    {
        using ObjectIdCollection xrefIds = new();
        GetAllXrefNode(xrefIds);
        foreach (ObjectId id in xrefIds)
            _tr.Database.DetachXref(id);
    }

    /// <summary>
    /// 重载
    /// </summary>
    public void Reload()
    {
        using ObjectIdCollection xrefIds = new();
        GetAllXrefNode(xrefIds);
        if (xrefIds.Count > 0)
            _tr.Database.ReloadXrefs(xrefIds);
    }

    /// <summary>
    /// 卸载
    /// </summary>
    public void Unload()
    {
        using ObjectIdCollection xrefIds = new();
        GetAllXrefNode(xrefIds);
        if (xrefIds.Count > 0)
            _tr.Database.UnloadXrefs(xrefIds);
    }

    #endregion

    #region 双重绑定

    /// <summary>
    /// 获取参照
    /// </summary>
    /// <param name="xrefIds">返回全部参照id</param>
    private void GetAllXrefNode(ObjectIdCollection xrefIds)
    {
        // 储存要处理的参照id
        //var xrefIds = new ObjectIdCollection();
        XrefNodeForEach((xNodeName, xNodeId, _, _) =>
        {
            if (XrefNamesContains(xNodeName))
                xrefIds.Add(xNodeId);
        });
    }

    private bool XrefNamesContains(string xNodeName)
    {
        // 为空的时候全部加入 || 有内容时候含有目标
        return _xrefNames is null || _xrefNames.Contains(xNodeName);
    }

    /// <summary>
    /// 遍历参照
    /// </summary>
    /// <param name="action">(参照名,参照块表记录id,参照状态,是否嵌入)</param>
    private void XrefNodeForEach(Action<string, ObjectId, XrefStatus, bool> action)
    {
        // btRec.IsFromOverlayReference 是覆盖
        // btRec.GetXrefDatabase(true) 外部参照数据库


        // 解析外部参照:此功能不能锁定文档
        _tr.Database.ResolveXrefs(UseThreadEngine, DoNewOnly);

        var xg = _tr.Database.GetHostDwgXrefGraph(IncludeGhosts);
        for (var i = 0; i < xg.NumNodes; i++)
        {
            var xNode = xg.GetXrefNode(i);
            if (!xNode.BlockTableRecordId.IsOk())
                continue;

            action.Invoke(xNode.Name, xNode.BlockTableRecordId, xNode.XrefStatus, xNode.IsNested);
        }
    }

    /// <summary>
    /// 符号表记录加入容器
    /// </summary>
    private static void AddedXBindIds<TTable, TRecord>(ObjectIdCollection xbindXrefsIds,
        SymbolTable<TTable, TRecord> symbolTable) where TTable : SymbolTable
        where TRecord : SymbolTableRecord, new()
    {
        symbolTable.ForEach(tabRec =>
        {
            if (tabRec.IsResolved)
                xbindXrefsIds.Add(tabRec.ObjectId);
        }, checkIdOk: true);
    }


    private void GetXBindIds(ObjectIdCollection xbindIds)
    {
        // xbind
        // 0x01 它是用来绑其他符号表,绑块表会有异常
        // 0x02 集合若有问题,就会出现eWrongObjectType
        //var xbindIds = new ObjectIdCollection();

        // 起初测试是将九大符号表记录均加入的,但经实测不行...(为什么?存疑)

        #region Option1

        if ((SymModesBind & SymModes.LayerTable) == SymModes.LayerTable)
            AddedXBindIds(xbindIds, _tr.LayerTable);

        if ((SymModesBind & SymModes.TextStyleTable) == SymModes.TextStyleTable)
            AddedXBindIds(xbindIds, _tr.TextStyleTable);

        if ((SymModesBind & SymModes.RegAppTable) == SymModes.RegAppTable)
            AddedXBindIds(xbindIds, _tr.RegAppTable);

        if ((SymModesBind & SymModes.DimStyleTable) == SymModes.DimStyleTable)
            AddedXBindIds(xbindIds, _tr.DimStyleTable);

        if ((SymModesBind & SymModes.LinetypeTable) == SymModes.LinetypeTable)
            AddedXBindIds(xbindIds, _tr.LinetypeTable);

        #endregion

        #region Option2

        if ((SymModesBind & SymModes.UcsTable) == SymModes.UcsTable)
            AddedXBindIds(xbindIds, _tr.UcsTable);

        if ((SymModesBind & SymModes.ViewTable) == SymModes.ViewTable)
            AddedXBindIds(xbindIds, _tr.ViewTable);

        if ((SymModesBind & SymModes.ViewportTable) == SymModes.ViewportTable)
            AddedXBindIds(xbindIds, _tr.ViewportTable);

        #endregion
    }

    private void GetBindIds(ObjectIdCollection bindIds)
    {
        // bind 只绑块表
        //var bindIds = new ObjectIdCollection();

        _tr.BlockTable.ForEach(btr =>
        {
            if (btr.IsLayout)
                return;

            // 外部参照 && 已融入
            if (btr is { IsFromExternalReference: true, IsResolved: true })
                bindIds.Add(btr.ObjectId);
        }, checkIdOk: true);
    }

    /// <summary>
    /// 获取可以拆离的ids
    /// </summary>
    /// <param name="nested">返回已卸载中含有嵌套的参照,要重载之后才能绑定</param>
    /// <returns>返回未参照中嵌套的参照,直接拆离</returns>
    private List<ObjectId> GetDetachIds(Dictionary<ObjectId, string> nested)
    {
        // 直接拆离的id
        List<ObjectId> detachIds = [];

        // 收集要处理的id
        XrefNodeForEach((xNodeName, xNodeId, xNodeStatus, xNodeIsNested) =>
        {
            switch (xNodeStatus)
            {
                case XrefStatus.Unresolved: // 未融入_ResolveXrefs参数2
                    break;
                case XrefStatus.FileNotFound: // 未融入(未解析)_未找到文件
                    break;
                case XrefStatus.Unreferenced: // 未参照
                {
                    // 为空的时候全部加入 || 有内容时候含有目标
                    if (XrefNamesContains(xNodeName))
                        detachIds.Add(xNodeId);
                }
                    break;
                case XrefStatus.Unloaded: // 已卸载
                {
                    // 为空的时候全部加入 || 有内容时候含有目标
                    if (XrefNamesContains(xNodeName))
                    {
                        var btr = _tr.GetObject<BlockTableRecord>(xNodeId);
                        if (btr != null && btr.IsFromExternalReference)
                        {
                            if (!xNodeIsNested)
                                detachIds.Add(xNodeId);
                            else if (!nested.ContainsKey(xNodeId))
                                nested.Add(xNodeId, xNodeName); // 嵌套参照
                        }
                    }
                }
                    break;
                case XrefStatus.Resolved: // 已融入_就是可以绑定的
                    break;
                case XrefStatus.NotAnXref: // 不是外部参照
                    break;
            }
        });
        return detachIds;
    }

    /// <summary>
    /// 双重绑定参照
    /// <a href="https://www.cnblogs.com/SHUN-ONCET/p/16593360.html">参考链接</a>
    /// </summary>
    private void DoubleBind()
    {
        Dictionary<ObjectId, string> nested = new();
        var detachIds = GetDetachIds(nested);

        // 拆离:未参照的文件
        if (AutoDetach)
        {
            for (var i = 0; i < detachIds.Count; i++)
                _tr.Database.DetachXref(detachIds[i]);
        }

        // 重载:嵌套参照已卸载了,需要重载之后才能进行绑定
        var keys = nested.Keys;
        if (keys.Count > 0)
        {
            using ObjectIdCollection idc = new(keys.ToArray());
            _tr.Database.ReloadXrefs(idc);
        }

        // 绑定:切勿交换,否则会绑定无效
        using ObjectIdCollection bindIds = new();
        using ObjectIdCollection xbindIds = new();
        GetBindIds(bindIds);
        GetXBindIds(xbindIds);
        if (xbindIds.Count > 0)
            _tr.Database.XBindXrefs(xbindIds, BindOrInsert);
        if (bindIds.Count > 0)
            _tr.Database.BindXrefs(bindIds, BindOrInsert);


        // 内部删除嵌套参照的块操作
        if (EraseNested)
        {
            foreach (var item in nested)
            {
                var name = item.Value;
                if (_tr.BlockTable.Has(name))
                    _tr.GetObject<BlockTableRecord>(_tr.BlockTable[name], OpenMode.ForWrite)?.Erase();
            }
        }
    }

    #endregion
}
