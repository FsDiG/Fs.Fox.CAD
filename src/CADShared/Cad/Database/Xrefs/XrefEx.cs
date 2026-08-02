namespace Fs.Fox.Cad;

/// <summary>
/// 参照扩展
/// </summary>
public static class XrefEx
{
    /// <summary>
    /// 外部参照工厂
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xrefModes">处理参照的枚举</param>
    /// <param name="xrefNames">要处理的参照名称,<see langword="null"/>就处理所有</param>
    public static void XrefFactory(this DBTrans tr, XrefModes xrefModes, HashSet<string>? xrefNames = null)
    {
        var xf = new XrefFactory(tr, xrefNames);
        tr.Task(() =>
        {
            switch (xrefModes)
            {
                case XrefModes.Unload:
                    xf.Unload();
                    break;
                case XrefModes.Reload:
                    xf.Reload();
                    break;
                case XrefModes.Detach:
                    xf.Detach();
                    break;
                case XrefModes.Bind:
                    xf.Bind();
                    break;
            }
        });
    }
}
