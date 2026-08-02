namespace Fs.Fox.Cad;

/// <summary>
/// Dynamic block visibility parameter metadata.
/// </summary>
public class BlockVisibilityInfo
{
    /// <summary>
    /// Gets or sets whether the block definition contains a visibility parameter.
    /// </summary>
    public bool Has { get; set; }

    /// <summary>
    /// Gets or sets the visibility property name.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the allowed visibility values in definition order.
    /// </summary>
    public List<string> AllowedValues { get; set; } = new();
}
