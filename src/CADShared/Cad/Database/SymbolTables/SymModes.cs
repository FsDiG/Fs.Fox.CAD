namespace Fs.Fox.Cad;

/// <summary>
/// 符号表模式
/// </summary>
[Flags]
public enum SymModes : ushort
{
    /// <summary>
    /// 块表
    /// </summary>
    BlockTable = 1,

    /// <summary>
    /// 图层表
    /// </summary>
    LayerTable = 2,
    /// <summary>
    /// 文字样式表
    /// </summary>
    TextStyleTable = 4,
    /// <summary>
    /// 注册应用程序表
    /// </summary>
    RegAppTable = 8,
    /// <summary>
    /// 标注样式表
    /// </summary>
    DimStyleTable = 16,
    /// <summary>
    /// 线型表
    /// </summary>
    LinetypeTable = 32,
    /// <summary>
    /// 图层|字体|标注|线型|应用
    /// </summary>
    Option1 = LayerTable | TextStyleTable | DimStyleTable | LinetypeTable | RegAppTable,

    /// <summary>
    /// 用户坐标系表
    /// </summary>
    UcsTable = 64,
    /// <summary>
    /// 视图表
    /// </summary>
    ViewTable = 128,
    /// <summary>
    /// 视口表
    /// </summary>
    ViewportTable = 256,
    /// <summary>
    /// 坐标|视口|视图
    /// </summary>
    Option2 = UcsTable | ViewTable | ViewportTable,

    /// <summary>
    /// 全部
    /// </summary>
    All = BlockTable | Option1 | Option2
}
