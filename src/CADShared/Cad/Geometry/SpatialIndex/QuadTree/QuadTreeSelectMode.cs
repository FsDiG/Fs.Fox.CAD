namespace Fs.Fox.Cad;

/// <summary>
/// 四叉树选择模式
/// </summary>
public enum QuadTreeSelectMode
{
    /// <summary>
    /// 碰撞到就选中
    /// </summary>
    IntersectsWith, 
    /// <summary>
    /// 全包含才选中
    /// </summary>
    Contains,     
}
