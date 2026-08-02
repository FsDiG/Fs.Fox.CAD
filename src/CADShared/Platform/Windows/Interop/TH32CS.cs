#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#if true
namespace Fs.Fox.Basal;

// https://blog.csdn.net/qq_43812868/article/details/108587936
[Flags]
public enum TH32CS : uint
{
    /// <summary>
    /// 原因在于如果不采用改参数的话,有可能快照会占用整个堆的空间
    /// </summary>
    TH32CS_SNAPNOHEAPS = 0x40000000,
    /// <summary>
    /// 声明快照句柄是可继承的
    /// </summary>
    TH32CS_INHERIT = 0x80000000,
    /// <summary>
    /// 在快照中包含在th32ProcessID中指定的进程的所有的堆
    /// </summary>
    TH32CS_SNAPHEAPLIST = 0x00000001,
    /// <summary>
    /// 在快照中包含系统中所有的进程
    /// </summary>
    TH32CS_SNAPPROCESS = 0x00000002,
    /// <summary>
    /// 在快照中包含系统中所有的线程
    /// </summary>
    TH32CS_SNAPTHREAD = 0x00000004,
    /// <summary>
    /// 在快照中包含在th32ProcessID中指定的进程的所有的模块
    /// </summary>
    TH32CS_SNAPMODULE = 0x00000008,
    /// <summary>
    /// 在快照中包含系统中所有的进程和线程
    /// </summary>
    TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD | TH32CS_SNAPMODULE,
}
#endif
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
