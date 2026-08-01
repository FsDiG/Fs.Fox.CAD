#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#if true
namespace Fs.Fox.Basal;

// https://blog.csdn.net/biyusr/article/details/108376195
public enum MOUSEEVENTF : int
{
    /// <summary>
    /// 移动鼠标
    /// </summary>
    MOVE = 0x0001,
    /// <summary>
    /// 模拟鼠标左键按下
    /// </summary>
    LEFTDOWN = 0x0002,
    /// <summary>
    /// 模拟鼠标左键抬起
    /// </summary>
    LEFTUP = 0x0004,
    /// <summary>
    /// 模拟鼠标右键按下
    /// </summary>
    RIGHTDOWN = 0x0008,
    /// <summary>
    /// 模拟鼠标右键抬起
    /// </summary>
    RIGHTUP = 0x0010,
    /// <summary>
    /// 模拟鼠标中键按下
    /// </summary>
    MIDDLEDOWN = 0x0020,
    /// <summary>
    /// 模拟鼠标中键抬起
    /// </summary>
    MIDDLEUP = 0x0040,
    /// <summary>
    /// 标示是否采用绝对坐标
    /// </summary>
    ABSOLUTE = 0x8000,
    /// <summary>
    /// 模拟鼠标滚轮滚动操作,必须配合dwData参数
    /// </summary>
    WHEEL = 0x0800,
}
#endif
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
