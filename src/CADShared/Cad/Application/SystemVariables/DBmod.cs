namespace Fs.Fox.Cad;

/// <summary>
/// 获取数据库修改状态
/// <a href="https://knowledge.autodesk.com/support/autocad/learn-explore/caas/CloudHelp/cloudhelp/2020/ENU/AutoCAD-Core/files/GUID-E255E808-2D48-4BDE-A760-FFEA28E5A86F-htm.html">
/// 相关链接</a>
/// </summary>
[Flags]
public enum DBmod : short
{
    /// <summary>
    /// 数据库未修改
    /// </summary>
    [Description("数据库未修改")]
    DatabaseNoModifies = 0,
    /// <summary>
    /// 数据库有修改
    /// </summary>
    [Description("数据库有修改")]
    Database = 1,
    /// <summary>
    /// 变量有修改
    /// </summary>
    [Description("变量有修改")]
    Value = 4,
    /// <summary>
    /// 窗口有修改
    /// </summary>
    [Description("窗口有修改")]
    Window = 8,
    /// <summary>
    /// 视图有修改
    /// </summary>
    [Description("视图有修改")]
    View = 16,
    /// <summary>
    /// 字段有修改
    /// </summary>
    [Description("字段有修改")]
    Field = 32
}
