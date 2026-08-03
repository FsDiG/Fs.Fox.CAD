#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// 临时修改 CAD 系统变量，并在释放作用域时恢复原值。
/// </summary>
/// <remarks>
/// 作用域必须在同一个 CAD 命令和活动文档上下文内使用。CAD 系统变量可能属于文档级状态；
/// 如果释放时活动文档已经变化，本类型会拒绝恢复并抛出 <see cref="InvalidOperationException"/>，
/// 以免把原值写入错误的文档。恢复创建作用域时的文档上下文后，可以再次调用 <see cref="Dispose"/>。
/// <para>
/// 捕获的值不会进行数值类型转换，因为 CAD 宿主在恢复时可能要求保留原始运行时类型，
/// 例如必须使用 <see cref="short"/> 而不是 <see cref="int"/>。
/// </para>
/// </remarks>
public sealed class SystemVariableScope : IDisposable
{
    private readonly string _variableName;
    private readonly object _originalValue;
    private readonly Document? _document;
    private bool _disposed;

    /// <summary>
    /// 获取构造函数接收的系统变量名。
    /// </summary>
    public string VariableName => _variableName;

    /// <summary>
    /// 获取应用临时值之前捕获的原值。
    /// </summary>
    public object OriginalValue => _originalValue;

    /// <summary>
    /// 捕获当前值并应用临时值。
    /// </summary>
    /// <param name="variableName">CAD 系统变量名。</param>
    /// <param name="temporaryValue">在本作用域生命周期内使用的临时值。</param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="variableName"/> 或 <paramref name="temporaryValue"/> 为 <see langword="null"/>。
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="variableName"/> 为空或仅包含空白字符。</exception>
    public SystemVariableScope(string variableName, object temporaryValue)
    {
        ArgumentNullException.ThrowIfNull(variableName);
        ArgumentNullException.ThrowIfNull(temporaryValue);
        if (string.IsNullOrWhiteSpace(variableName))
            throw new ArgumentException("系统变量名不能为空或仅包含空白字符。", nameof(variableName));

        _variableName = variableName;
        _document = CadCoreApp.DocumentManager.MdiActiveDocument;
        _originalValue = CadCoreApp.GetSystemVariable(variableName);

        if (!Equals(_originalValue, temporaryValue))
            CadCoreApp.SetSystemVariable(variableName, temporaryValue);
    }

    /// <summary>
    /// 恢复捕获的原值；成功恢复后重复调用不会产生影响。
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// 当前活动文档与创建作用域时不同。此时不会恢复变量，也不会将作用域标记为已释放；
    /// 恢复原文档上下文后可以重试。
    /// </exception>
    public void Dispose()
    {
        if (_disposed)
            return;

        // Get/SetSystemVariable 会针对当前活动文档解析文档级变量；先校验上下文，
        // 避免把捕获的原值恢复到其他文档。
        if (!ReferenceEquals(CadCoreApp.DocumentManager.MdiActiveDocument, _document))
            throw new InvalidOperationException("活动文档上下文已经变化；请恢复创建作用域时的文档上下文后重试。");

        var currentValue = CadCoreApp.GetSystemVariable(_variableName);
        if (!Equals(currentValue, _originalValue))
            CadCoreApp.SetSystemVariable(_variableName, _originalValue);

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
