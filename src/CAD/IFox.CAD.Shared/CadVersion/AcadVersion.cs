namespace IFoxCAD.Cad;

/// <summary>
/// cad版本号类
/// </summary>
public static class AcadVersion
{
    private static readonly string _pattern = @"Autodesk\\AutoCAD\\R(\d+)\.(\d+)\\.*?";

    /// <summary>
    /// 所有安装的cad的版本号
    /// </summary>
    public static List<CadVersion> Versions
    {
        get
        {
            string[] copys = Registry.LocalMachine
                .OpenSubKey(@"SOFTWARE\Autodesk\Hardcopy")!
                            .GetValueNames();

            List<CadVersion> _versions = [];
            _versions.AddRange(from t in copys
                where Regex.IsMatch(t, _pattern)
                let gs = Regex.Match(t, _pattern).Groups
                select new CadVersion
                {
                    ProductRootKey = t,
                    ProductName = Registry.LocalMachine.OpenSubKey("SOFTWARE")!.OpenSubKey(t)
                        ?.GetValue("ProductName")
                        .ToString(),
                    Major = int.Parse(gs[1].Value),
                    Minor = int.Parse(gs[2].Value),
                });
            return _versions;
        }
    }

    /// <summary>已打开的cad的版本号</summary>
    /// <param name="app">已打开cad的application对象</param>
    /// <returns>cad版本号对象</returns>
    public static CadVersion? FromApp(object app)
    {
        ArgumentNullEx.ThrowIfNull(app);

        var acver = app.GetType()
                        .InvokeMember(
                            "Version",
                            BindingFlags.GetProperty,
                            null,
                            app, []).ToString();

        var gs = Regex.Match(acver, @"(\d+)\.(\d+).*?").Groups;
        var major = int.Parse(gs[1].Value);
        var minor = int.Parse(gs[2].Value);
        return Versions.FirstOrDefault(t => t.Major == major && t.Minor == minor);
    }
}