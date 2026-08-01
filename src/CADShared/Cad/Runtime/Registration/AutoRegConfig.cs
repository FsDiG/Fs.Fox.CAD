namespace Fs.Fox.Cad;

/// <summary>
/// 注册中心配置信息
/// </summary>
[Flags]
public enum AutoRegConfig
{
    /// <summary>
    /// 不进行任何操作
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// 注册表
    /// </summary>
    Regedit = 1,

    /// <summary>
    /// 反射特性
    /// </summary>
    ReflectionAttribute = 2,

    /// <summary>
    /// 反射接口
    /// </summary>
    ReflectionInterface = 4,

    /// <summary>
    /// 移除教育版
    /// </summary>
    RemoveEMR = 8,

    /// <summary>
    /// 全部
    /// </summary>
    All = Regedit | ReflectionAttribute | ReflectionInterface | RemoveEMR,
}
