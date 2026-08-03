#if AC_NET48 || ZWCAD || GC_2022 || GC_2023
using ArgumentNullException = Fs.Fox.Basal.ArgumentNullEx;
#endif

namespace Fs.Fox.Cad;

/// <summary>
/// Temporarily changes a CAD system variable and restores its original value when disposed.
/// </summary>
/// <remarks>
/// Keep the scope inside the same CAD command and document context. CAD system variables can be
/// document-scoped, so switching the active document before disposal can restore the value in the
/// wrong context.
/// <para>
/// The captured value is retained without numeric conversion because CAD hosts can require the
/// original runtime type (for example, <see cref="short"/> instead of <see cref="int"/>) on restore.
/// </para>
/// </remarks>
public sealed class SystemVariableScope : IDisposable
{
    private readonly string _variableName;
    private readonly object _originalValue;
    private bool _disposed;

    /// <summary>
    /// Gets the system variable name supplied to the constructor.
    /// </summary>
    public string VariableName => _variableName;

    /// <summary>
    /// Gets the value captured before the temporary value was applied.
    /// </summary>
    public object OriginalValue => _originalValue;

    /// <summary>
    /// Captures the current value and applies a temporary value.
    /// </summary>
    /// <param name="variableName">CAD system variable name.</param>
    /// <param name="temporaryValue">Value to keep for the lifetime of this scope.</param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="variableName"/> or <paramref name="temporaryValue"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="variableName"/> is empty or whitespace.</exception>
    public SystemVariableScope(string variableName, object temporaryValue)
    {
        ArgumentNullException.ThrowIfNull(variableName);
        ArgumentNullException.ThrowIfNull(temporaryValue);
        if (string.IsNullOrWhiteSpace(variableName))
            throw new ArgumentException("System variable name cannot be empty or whitespace.", nameof(variableName));

        _variableName = variableName;
        _originalValue = CadCoreApp.GetSystemVariable(variableName);

        if (!Equals(_originalValue, temporaryValue))
            CadCoreApp.SetSystemVariable(variableName, temporaryValue);
    }

    /// <summary>
    /// Restores the captured value. Repeated calls after a successful restore have no effect.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        // CadCoreApp resolves document-scoped variables against the active document. The scope must
        // therefore be disposed in the same document context in which it was constructed.
        var currentValue = CadCoreApp.GetSystemVariable(_variableName);
        if (!Equals(currentValue, _originalValue))
            CadCoreApp.SetSystemVariable(_variableName, _originalValue);

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
