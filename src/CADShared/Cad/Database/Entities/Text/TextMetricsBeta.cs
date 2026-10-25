using ArgumentException = System.ArgumentException;
using ArgumentNullException = System.ArgumentNullException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Fs.Fox.Cad;

/// <summary>
/// 使用指定数据库的文字样式测量单行文字。Beta：尚待真实 CAD 宿主验收。
/// </summary>
/// <remarks>
/// 能力来源：张帆的 ZFGK.AutoCAD TextUtil.GetTextHeightAndWidth；本实现重新定义输入、字体失败和资源归属。
/// 只能在 CAD 宿主允许的主线程执行上下文调用，调用方负责文档上下文和必要的锁。
/// 测量期间会临时切换 WorkingDatabase，并在正常及异常退出时恢复；不是并发安全的离线字体测量器。
/// </remarks>
public static class TextMetricsBeta
{
    /// <summary>
    /// 返回单行文字在文字自身局部坐标中的二维范围，不创建或修改数据库实体。
    /// </summary>
    /// <param name="database">提供字体及样式上下文的目标数据库，由调用方拥有。</param>
    /// <param name="textStyleId">目标数据库中有效的文字样式 ID；不使用当前默认样式代替无效 ID。</param>
    /// <param name="text">单行内容；不解析或删除 MText 花括号、上下标格式。空串返回原点零范围。</param>
    /// <param name="height">有限且大于零的字高，替代样式默认字高。</param>
    /// <param name="widthFactor">有限且大于零的宽度因子，替代样式默认宽度因子。</param>
    /// <param name="includeSpaces">是否将首尾空格的进笔距离计入范围。</param>
    /// <param name="raw">为 true 时按字面测量；为 false 时由宿主解释 DBText 的 %% 转义。</param>
    /// <returns>文字局部范围；宽和高分别为 MaxPoint 与 MinPoint 的 X、Y 差。</returns>
    /// <exception cref="ArgumentNullException">数据库或文本为 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">字高或宽度因子不是有限正数。</exception>
    /// <exception cref="ArgumentException">样式无效、属于其他数据库、不是文字样式，或文本包含控制字符/换行符。</exception>
    /// <exception cref="System.InvalidOperationException">字体加载失败、宿主报告替代字体、未确认加载成功，或返回无效范围。</exception>
    /// <exception cref="System.NotSupportedException">ZW_2022、GC_2022、GC_2023 编译目标缺少完整的 GI 样式转换/测量 API；在参数校验及状态切换前抛出。</exception>
    /// <remarks>
    /// 返回范围取决于实际宿主及可用字体。若字体或大字体加载失败、被替代，或没有成功状态位，抛出异常。
    /// 其他宿主 SDK 异常保留原样传播。样式记录只读，字高和宽度修改仅作用于临时 GraphicsInterface.TextStyle。
    /// 空串仍验证数据库和样式，但不加载字体。此方法不计算 MText 布局、实体旋转、对齐或 WCS 包围框。
    /// </remarks>
    public static Extents2d MeasureSingleLine(Database database, ObjectId textStyleId, string text,
        double height, double widthFactor = 1, bool includeSpaces = true, bool raw = false)
    {
#if ZW_2022 || GC_2022 || GC_2023
        throw new NotSupportedException("当前编译目标的 SDK 不提供完整的 GI 文字样式转换与范围测量 API，暂不支持此测量接口。");
#else
        if (database is null)
            throw new ArgumentNullException(nameof(database));
        if (text is null)
            throw new ArgumentNullException(nameof(text));
        ValidatePositiveFinite(height, nameof(height));
        ValidatePositiveFinite(widthFactor, nameof(widthFactor));
        foreach (var character in text)
        {
            if (char.IsControl(character) || character == '\u2028' || character == '\u2029')
                throw new ArgumentException("文字必须是单行内容，不能包含控制字符或换行符。", nameof(text));
        }

        if (textStyleId.IsNull || !textStyleId.IsValid || textStyleId.IsErased || textStyleId.Database != database)
            throw new ArgumentException("文字样式必须属于指定数据库且有效。", nameof(textStyleId));

        // Dispose in reverse order: finish the native read transaction before restoring WorkingDatabase.
        using var databaseScope = new SwitchDatabase(database);
        using var transaction = database.TransactionManager.StartTransaction();
        if (transaction.GetObject(textStyleId, OpenMode.ForRead) is not TextStyleTableRecord)
            throw new ArgumentException("指定的对象不是文字样式。", nameof(textStyleId));

        if (text.Length == 0)
            return new Extents2d(Point2d.Origin, Point2d.Origin);

        using var graphicsStyle = new CadGI.TextStyle();
        graphicsStyle.FromTextStyleTableRecord(textStyleId);
        graphicsStyle.TextSize = height;
        graphicsStyle.XScale = widthFactor;

        // AcGiTextStyle::loadStyleRec: 0x02/0x04 = 字体/大字体加载失败，0x08/0x10 = 使用了替代字体。
        // 0x01 表示已加载。0x00 未被官方契约定义为成功；也不能把掩码简单转换为 bool。
        var loadStatus = graphicsStyle.LoadStyleRec;
        if ((loadStatus & 0x1e) != 0 || (loadStatus & 0x01) == 0)
            throw new InvalidOperationException($"文字样式字体未确认成功加载或发生替代，状态码 0x{loadStatus:X2}。");

        var bounds = graphicsStyle.ExtentsBox(text, includeSpaces, raw, null);
        var width = bounds.MaxPoint.X - bounds.MinPoint.X;
        var measuredHeight = bounds.MaxPoint.Y - bounds.MinPoint.Y;
        if (!IsFinite(bounds.MinPoint.X) || !IsFinite(bounds.MinPoint.Y)
            || !IsFinite(bounds.MaxPoint.X) || !IsFinite(bounds.MaxPoint.Y)
            || !IsFinite(width) || !IsFinite(measuredHeight) || width < 0 || measuredHeight < 0)
            throw new InvalidOperationException("宿主返回了无效的文字测量范围。");

        return bounds;
#endif
    }

    private static void ValidatePositiveFinite(double value, string name)
    {
        if (!IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(name, "必须为有限正数。");
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
