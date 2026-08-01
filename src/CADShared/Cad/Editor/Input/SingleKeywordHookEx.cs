namespace Fs.Fox.Cad;

/// <summary>
/// 单文本关键字钩子扩展
/// </summary>
public static class SingleKeywordHookEx
{
    /// <summary>
    /// 钩取单文本关键字
    /// </summary>
    /// <param name="keywords">关键字集合</param>
    /// <param name="workType">工作模式</param>
    /// <returns>单文本关键字类(需要using)</returns>
    public static SingleKeyWordHook HookSingleKeyword(this KeywordCollection keywords, SingleKeyWordWorkType workType = SingleKeyWordWorkType.WRITE_LINE)
    {
        var singleKeyWordHook = new SingleKeyWordHook(workType);
        singleKeyWordHook.AddKeys(keywords);
        return singleKeyWordHook;
    }
}
