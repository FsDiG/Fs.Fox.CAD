// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Fs.Fox.Basal;

#line hidden // 调试的时候跳过它
/// <summary>
/// 控制循环结束
/// </summary>
public class LoopState
{
    private const int PlsNone = 0;
    private const int PlsExceptional = 1;
    private const int PlsBroken = 2;
    private const int PlsStopped = 4;
    private const int PlsCanceled = 8;

    private volatile int _flag = PlsNone;

    public bool IsRun => _flag == PlsNone;
    public bool IsExceptional => (_flag & PlsExceptional) == PlsExceptional;
    public bool IsBreak => (_flag & PlsBroken) == PlsBroken;
    public bool IsStop => (_flag & PlsStopped) == PlsStopped;
    public bool IsCancel => (_flag & PlsCanceled) == PlsCanceled;

    public void Exceptional()
    {
        if ((_flag & PlsExceptional) != PlsExceptional)
            _flag |= PlsExceptional;
    }
    public void Break() => _flag = PlsBroken;
    public void Stop() => _flag = PlsStopped;
    public void Cancel() => _flag = PlsCanceled;
    public void Reset() => _flag = PlsNone;
}
#line default

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
