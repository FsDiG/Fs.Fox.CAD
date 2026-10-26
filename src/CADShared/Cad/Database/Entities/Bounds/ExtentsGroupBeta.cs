namespace Fs.Fox.Cad;

/// <summary>原始范围集合中的一个 XY 邻近连通分量。Beta：实际 CAD 宿主行为尚待验收。</summary>
public sealed class ExtentsGroupBeta
{
    /// <summary>该组所有输入范围的三维合并包围框。</summary>
    public Extents3d Bounds { get; }

    /// <summary>组内输入范围的原始索引，按输入顺序排列；重复范围仍有各自的索引。</summary>
    public IReadOnlyList<int> SourceIndices { get; }

    internal ExtentsGroupBeta(OrderingAndGroupingCoreBeta.Group group)
    {
        var box = group.Bounds;
        Bounds = new Extents3d(new Point3d(box.MinX, box.MinY, box.MinZ),
            new Point3d(box.MaxX, box.MaxY, box.MaxZ));
        // 包装只读接口还不够，先复制内部数组，结果不能与计算核心共享可变成员存储。
        SourceIndices = Array.AsReadOnly((int[])group.Indices.Clone());
    }
}
