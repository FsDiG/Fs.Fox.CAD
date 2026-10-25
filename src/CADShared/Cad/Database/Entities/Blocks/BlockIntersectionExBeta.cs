using ArgumentException = System.ArgumentException;
using ArgumentNullException = System.ArgumentNullException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

// 来源参考：Fs.Zfgk.CAD a882003436fb4cc31b24c90f6779177c13a60abe，ObjectARX/Entity/BlockUtil.cs。
// 原文件头：Author TianTeng；Copyright © 北京智帆高科科技有限公司 2015。本文件按新 Beta 契约重新实现。

namespace Fs.Fox.Cad;

/// <summary>查询普通块内实际曲线与 WCS 曲线的交点。Beta：真实 CAD 宿主验收尚未运行。</summary>
/// <remarks>
/// 能力来源：Zfgk BlockUtil.IntersectWith；本实现增加显式事务、嵌套定义路径检查和临时对象释放。
/// 查询存储几何，不按实体、图层或视口可见性筛选；文字、属性和其他非 Curve 叶节点不参与。
/// </remarks>
public static class BlockIntersectionExBeta
{
    /// <summary>按深度优先的块定义实体顺序，返回去重后的真实三维 WCS 交点快照。</summary>
    /// <param name="block">直接位于模型空间或图纸空间的普通块参照，由调用方拥有。</param>
    /// <param name="transaction">根块数据库当前最上层的有效事务，由调用方拥有。</param>
    /// <param name="target">同一空间中的曲线，或调用方持有且已使用 WCS 坐标的未入库曲线。</param>
    /// <param name="mode">只接受 OnBothOperands，或仅延伸 target 的 ExtendArgument；默认不延伸。</param>
    /// <param name="tolerance">有限且非负的 WCS 交点去重距离；零表示各坐标分量精确相等。</param>
    /// <returns>独立的只读点快照；无交点或无曲线叶节点时为空。</returns>
    /// <remarks>
    /// 仅支持原生 Line、Arc、Circle、Ellipse、轻量 Polyline、Spline、Ray、Xline 曲线类型。
    /// 动态块、MInsertBlock、外参、循环定义、旧式 Polyline2d/Polyline3d 及其他曲线类型明确拒绝。
    /// 嵌套变换按父矩阵乘子矩阵累计；非均匀变换由原生 GetTransformedCopy 处理，失败原样传播。
    /// 不提供 XY 投影、裁剪或可见几何求交，不以块包围框替代曲线。重合区段沿用 SDK 离散点结果。
    /// 按首次命中点保留结果，后续点只与已保留点比较；容差不传给原生求交，也不进行链式聚合。
    /// 单条曲线的点顺序由宿主决定，不保证不同宿主给出相同顺序或完全一致的数值。
    /// 所有数据库对象仅通过原生事务 ForRead 读取，不升级写状态，不提交、中止或释放调用方事务。
    /// 不修改或释放输入对象，不切换 WorkingDatabase，不访问 Editor；临时变换副本和点集合均在退出时释放。
    /// 调用方负责 CAD 线程、文档上下文及必要的访问条件；不可用作无宿主的离线求交器。
    /// </remarks>
    /// <exception cref="ArgumentNullException">根块、事务或目标曲线为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">mode 不受支持，或 tolerance 不是有限非负数。</exception>
    /// <exception cref="ArgumentException">根块未入库、空间/数据库身份不一致，或事务不是当前最上层事务。</exception>
    /// <exception cref="System.NotSupportedException">遇到不支持的块或曲线类型、外参，或变换副本不是曲线。</exception>
    /// <exception cref="System.InvalidOperationException">块定义路径循环，或宿主返回非有限交点。</exception>
    /// <exception cref="System.ObjectDisposedException">输入根块或目标曲线已释放。</exception>
    public static IReadOnlyList<Point3d> GetCurveIntersectionsBeta(this BlockReference block,
        Transaction transaction, Curve target, Intersect mode = Intersect.OnBothOperands,
        double tolerance = 1e-6)
    {
        if (block is null) throw new ArgumentNullException(nameof(block));
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (block.IsDisposed) throw new ObjectDisposedException(nameof(block));
        if (target.IsDisposed) throw new ObjectDisposedException(nameof(target));
        if (!Finite(tolerance) || tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "去重距离必须为有限非负数。");
        if (mode != Intersect.OnBothOperands && mode != Intersect.ExtendArgument)
            throw new ArgumentOutOfRangeException(nameof(mode), "只支持不延伸或仅延伸目标曲线。");

        var database = block.Database;
        if (database is null || !ValidId(block.ObjectId, database))
            throw new ArgumentException("根块必须为已入库的有效块参照。", nameof(block));
        if (!ReferenceEquals(database.TransactionManager.TopTransaction, transaction))
            throw new ArgumentException("必须使用根块数据库当前最上层事务。", nameof(transaction));
        if (!ValidId(block.OwnerId, database) ||
            transaction.GetObject(block.OwnerId, OpenMode.ForRead) is not BlockTableRecord { IsLayout: true })
            throw new ArgumentException("根块必须直接位于模型空间或图纸空间。", nameof(block));
        if (target.Database is not null &&
            (target.Database != database || !ValidId(target.ObjectId, database) || target.OwnerId != block.OwnerId))
            throw new ArgumentException("已入库的目标曲线必须与根块位于同一数据库和同一空间。", nameof(target));
        RequireSupportedCurve(target);

        var result = new List<Point3d>();
        var path = new HashSet<ObjectId>();
        var stack = new Stack<TraversalFrame>();
        PushDefinition(block, Matrix3d.Identity);
        while (stack.Count != 0)
        {
            var frame = stack.Peek();
            if (frame.NextIndex == frame.Children.Length)
            {
                stack.Pop();
                path.Remove(frame.DefinitionId);
                continue;
            }

            var child = transaction.GetObject(frame.Children[frame.NextIndex++], OpenMode.ForRead);
            if (child is BlockReference nested)
            {
                PushDefinition(nested, frame.ToWorld);
                continue;
            }
            if (child is not Curve curve) continue;
            RequireSupportedCurve(curve);

            // 旧式带顶点子对象的曲线不进入此路径，避免原生浅克隆变换修改源对象。
            using var transformed = curve.GetTransformedCopy(frame.ToWorld);
            if (transformed is not Curve transformedCurve)
                throw new NotSupportedException("宿主未返回曲线变换副本，不能省略该叶节点的交点。");
            using var points = new Point3dCollection();
            transformedCurve.IntersectWith(target, mode, points, IntPtr.Zero, IntPtr.Zero);
            foreach (Point3d point in points)
            {
                if (!Finite(point.X) || !Finite(point.Y) || !Finite(point.Z))
                    throw new InvalidOperationException("宿主返回了非有限交点。");
                if (!result.Any(kept => WithinDistance(kept, point, tolerance)))
                    result.Add(point);
            }
        }
        return Array.AsReadOnly(result.ToArray());

        void PushDefinition(BlockReference reference, Matrix3d parentToWorld)
        {
            if (reference.GetType() != typeof(BlockReference) || reference.IsDynamicBlock)
                throw new NotSupportedException("只支持普通块参照，不支持动态块、阵列块或自定义块类型。");
            var definitionId = reference.BlockTableRecord;
            if (!ValidId(definitionId, database))
                throw new ArgumentException("块定义必须属于根块数据库且有效。", nameof(block));
            // 只检查当前路径；同一定义在其他位置的实例仍须按各自变换查询。
            if (!path.Add(definitionId))
                throw new InvalidOperationException("检测到循环的块定义路径。");
            var definition = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
            if (definition.IsFromExternalReference || definition.IsFromOverlayReference)
                throw new NotSupportedException("不支持外部参照，不能以未展开的外参返回部分交点。");
            // 显式栈避免有效但很深的嵌套耗尽 CLR 调用栈；子实体 ID 快照不持有原生枚举器。
            stack.Push(new TraversalFrame(definitionId, parentToWorld * reference.BlockTransform,
                definition.Cast<ObjectId>().ToArray()));
        }
    }

    private static void RequireSupportedCurve(Curve curve)
    {
        var type = curve.GetType();
        if (type != typeof(Line) && type != typeof(Arc) && type != typeof(Circle) &&
            type != typeof(Ellipse) && type != typeof(Polyline) && type != typeof(Spline) &&
            type != typeof(Ray) && type != typeof(Xline))
            throw new NotSupportedException("只支持 Line、Arc、Circle、Ellipse、轻量 Polyline、Spline、Ray 和 Xline 原生曲线。");
    }

    private static bool ValidId(ObjectId id, Database database) =>
        !id.IsNull && id.IsValid && !id.IsErased && id.Database == database;

    private static bool WithinDistance(Point3d first, Point3d second, double tolerance)
    {
        if (tolerance == 0)
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        var x = Math.Abs(first.X - second.X);
        var y = Math.Abs(first.Y - second.Y);
        var z = Math.Abs(first.Z - second.Z);
        if (x > tolerance || y > tolerance || z > tolerance) return false;
        // 先筛轴差再归一化，避免大坐标平方溢出；三个归一化轴差均不大于 1。
        x /= tolerance;
        y /= tolerance;
        z /= tolerance;
        return x * x + y * y + z * z <= 1;
    }

    private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    private sealed class TraversalFrame
    {
        internal readonly ObjectId DefinitionId;
        internal readonly Matrix3d ToWorld;
        internal readonly ObjectId[] Children;
        internal int NextIndex;

        internal TraversalFrame(ObjectId definitionId, Matrix3d toWorld, ObjectId[] children)
        {
            DefinitionId = definitionId;
            ToWorld = toWorld;
            Children = children;
        }
    }
}
