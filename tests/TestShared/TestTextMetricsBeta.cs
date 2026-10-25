#if ACAD
using NativeCadException = Autodesk.AutoCAD.Runtime.Exception;
#elif ZWCAD
using NativeCadException = ZwSoft.ZwCAD.Runtime.Exception;
#elif GC_2026
using NativeCadException = Gssoft.Gscad.Runtime.Exception;
#else
using NativeCadException = GrxCAD.Runtime.Exception;
#endif

namespace Test;

/// <summary>需要真实 CAD 宿主；构建通过不等于这些命令已执行。</summary>
public static class TestTextMetricsBeta
{
#if ZW_2022 || GC_2022 || GC_2023
    [CommandMethod(nameof(Test_TextMetricsBetaUnsupported))]
    public static void Test_TextMetricsBetaUnsupported()
    {
        var originalDatabase = HostApplicationServices.WorkingDatabase;
        AssertThrows<NotSupportedException>(() => TextMetricsBeta.MeasureSingleLine(null!, ObjectId.Null, "x", 1));
        Assert(HostApplicationServices.WorkingDatabase == originalDatabase, "不支持的目标不能改变工作数据库。");
        Env.Printl("TextMetricsBeta: 当前编译目标明确报告不支持且未改变工作数据库。");
    }
#else
    [CommandMethod(nameof(Test_TextMetricsBeta))]
    public static void Test_TextMetricsBeta()
    {
        var originalDatabase = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        var styleId = AddStyle(database, "FoxTextMetricsBetaArial", "arial.ttf");

        var standard = TextMetricsBeta.MeasureSingleLine(database, styleId, "Ag123", 5);
        var tall = TextMetricsBeta.MeasureSingleLine(database, styleId, "Ag123", 10);
        var wide = TextMetricsBeta.MeasureSingleLine(database, styleId, "Ag123", 5, 2);
        Assert(Width(standard) > 0 && Height(standard) > 0, "正常文字范围应为正数。");
        AssertClose(Width(tall), Width(standard) * 2, "字高翻倍时宽度应同步翻倍。");
        AssertClose(Height(tall), Height(standard) * 2, "字高翻倍时实际高度应翻倍。");
        AssertClose(Width(wide), Width(standard) * 2, "宽度因子翻倍时宽度应翻倍。");
        AssertClose(Height(wide), Height(standard), "宽度因子不应改变高度。");

        var literal = TextMetricsBeta.MeasureSingleLine(database, styleId, "{a^b}", 5, raw: true);
        var shortened = TextMetricsBeta.MeasureSingleLine(database, styleId, "a", 5, raw: true);
        Assert(Width(literal) > Width(shortened), "花括号和上标记号中的文本不能被删减为 a。");
        var withSpaces = TextMetricsBeta.MeasureSingleLine(database, styleId, "  Ag123  ", 5, includeSpaces: true);
        var withoutSpaces = TextMetricsBeta.MeasureSingleLine(database, styleId, "  Ag123  ", 5, includeSpaces: false);
        Assert(Width(withSpaces) > Width(withoutSpaces), "首尾空格选项应影响宽度。");
        var escaped = TextMetricsBeta.MeasureSingleLine(database, styleId, "%%d", 5, raw: false);
        var raw = TextMetricsBeta.MeasureSingleLine(database, styleId, "%%d", 5, raw: true);
        Assert(Width(raw) > Width(escaped), "按字面测量的 %%d 应宽于解释后的度数符号。");

        var empty = TextMetricsBeta.MeasureSingleLine(database, styleId, "", 5);
        Assert(Width(empty) == 0 && Height(empty) == 0, "空串应返回零范围。");
        using (var transaction = database.TransactionManager.StartTransaction())
        {
            var style = (TextStyleTableRecord)transaction.GetObject(styleId, OpenMode.ForRead);
            AssertClose(style.TextSize, 2, "测量不能改变样式默认字高。");
            AssertClose(style.XScale, 0.7, "测量不能改变样式默认宽度。");
        }

        Assert(HostApplicationServices.WorkingDatabase == originalDatabase, "成功路径应恢复工作数据库。");
        Env.Printl("TextMetricsBeta: 字体度量、转义、空格及样式只读检查通过。");
    }

    [CommandMethod(nameof(Test_TextMetricsBetaInvalidInput))]
    public static void Test_TextMetricsBetaInvalidInput()
    {
        var originalDatabase = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        using var otherDatabase = new Database(true, true);
        var styleId = database.Textstyle;

        AssertThrows<System.ArgumentNullException>(() => TextMetricsBeta.MeasureSingleLine(null!, styleId, "x", 1));
        AssertThrows<System.ArgumentNullException>(() => TextMetricsBeta.MeasureSingleLine(database, styleId, null!, 1));
        foreach (var value in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            AssertThrows<System.ArgumentOutOfRangeException>(() => TextMetricsBeta.MeasureSingleLine(database, styleId, "x", value));
            AssertThrows<System.ArgumentOutOfRangeException>(() => TextMetricsBeta.MeasureSingleLine(database, styleId, "x", 1, value));
        }
        foreach (var text in new[] { "a\nb", "a\rb", "a\tb", "a\0b", "a\u2028b", "a\u2029b" })
            AssertThrows<System.ArgumentException>(() => TextMetricsBeta.MeasureSingleLine(database, styleId, text, 1));

        AssertThrows<System.ArgumentException>(() => TextMetricsBeta.MeasureSingleLine(database, ObjectId.Null, "", 1));
        AssertThrows<System.ArgumentException>(() => TextMetricsBeta.MeasureSingleLine(database, otherDatabase.Textstyle, "", 1));
        AssertThrows<System.ArgumentException>(() => TextMetricsBeta.MeasureSingleLine(database, database.Clayer, "", 1));
        Assert(HostApplicationServices.WorkingDatabase == originalDatabase, "参数和样式失败路径应恢复工作数据库。");
        Env.Printl("TextMetricsBeta: 非法参数、样式身份及工作数据库恢复检查通过。");
    }

    [CommandMethod(nameof(Test_TextMetricsBetaMissingFont))]
    public static void Test_TextMetricsBetaMissingFont()
    {
        var originalDatabase = HostApplicationServices.WorkingDatabase;
        using var database = new Database(true, true);
        var missingFile = "fox_missing_font_" + Guid.NewGuid().ToString("N") + ".shx";
        var styleId = AddStyle(database, "FoxTextMetricsBetaMissing", missingFile);
        var rejected = false;
        try
        {
            TextMetricsBeta.MeasureSingleLine(database, styleId, "ABC", 5);
        }
        catch (InvalidOperationException exception)
        {
            Env.Printl("缺失字体异常：" + exception.Message);
            rejected = true;
        }
        catch (NativeCadException exception)
        {
            // 某些宿主在 FromTextStyleTableRecord 或 LoadStyleRec 中直接抛出原生异常。
            Env.Printl("缺失字体 SDK 异常：" + exception.Message);
            rejected = true;
        }

        Assert(rejected, "缺失字体必须报告加载失败或替代，不能静默返回其他字体的度量。");
        Assert(HostApplicationServices.WorkingDatabase == originalDatabase, "字体异常路径应恢复工作数据库。");
        using var transaction = database.TransactionManager.StartTransaction();
        var style = (TextStyleTableRecord)transaction.GetObject(styleId, OpenMode.ForRead);
        Assert(style.FileName == missingFile, "字体加载不能修改原样式记录。");
        Env.Printl("TextMetricsBeta: 缺失字体失败及状态恢复检查通过。");
    }

    private static ObjectId AddStyle(Database database, string name, string fileName)
    {
        using var transaction = database.TransactionManager.StartTransaction();
        var table = (TextStyleTable)transaction.GetObject(database.TextStyleTableId, OpenMode.ForWrite);
        using var style = new TextStyleTableRecord { Name = name, FileName = fileName, TextSize = 2, XScale = 0.7 };
        var id = table.Add(style);
        transaction.AddNewlyCreatedDBObject(style, true);
        transaction.Commit();
        return id;
    }
#endif

    private static double Width(Extents2d bounds) => bounds.MaxPoint.X - bounds.MinPoint.X;
    private static double Height(Extents2d bounds) => bounds.MaxPoint.Y - bounds.MinPoint.Y;

    private static void AssertClose(double actual, double expected, string message)
    {
        Assert(Math.Abs(actual - expected) <= 1e-6 * Math.Max(1, Math.Abs(expected)), message);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void AssertThrows<TException>(Action action) where TException : System.Exception
    {
        try { action(); }
        catch (TException) { return; }
        throw new InvalidOperationException("Expected " + typeof(TException).Name);
    }
}
