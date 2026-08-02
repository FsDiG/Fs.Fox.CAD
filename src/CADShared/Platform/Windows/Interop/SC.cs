#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#if true
namespace Fs.Fox.Basal;

[Flags]
public enum SC : uint
{
    // 窗体关闭消息
    SC_CLOSE = 0xf060,
    // 窗体最小化消息
    SC_MINIMIZE = 0xf020,
    // 窗体最大化消息
    SC_MAXIMIZE = 0xf030,
    // 窗体正常态消息 SC_RESTORE = 0xf120,
    SC_NOMAL = 0xf120,
}
#endif
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
