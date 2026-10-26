namespace Fs.Fox.Cad;

/// <summary>点与闭合轻量多段线区域的关系；Beta 数值及宿主边界见查询入口。</summary>
public enum PolylinePointRelationBeta
{
    /// <summary>位于偶奇填充区域外。</summary>
    Outside,
    /// <summary>位于偶奇填充区域内。</summary>
    Inside,
    /// <summary>在输入直线/圆弧边界的距离容差内；边界优先于填充规则。</summary>
    Boundary,
    /// <summary>到多段线基准平面的距离大于平面容差。</summary>
    OffPlane
}
