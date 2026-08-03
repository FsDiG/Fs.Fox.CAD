using System.Reflection;

namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Reads SDK members that are not present in every supported AutoCAD API
/// generation. Missing members are represented explicitly instead of removing
/// the corresponding Snoop row or failing the whole collector.
/// </summary>
internal static class OptionalApi
{
    internal static object GetPropertyValue(object instance, string propertyName)
    {
        if (instance is null)
            return "Not available: source object is null";

        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
            return $"Not available for {HostCapabilities.TargetApi}";

        try
        {
            return property.GetValue(instance, null) ?? "null";
        }
        catch (TargetInvocationException exception)
        {
            return $"Not available: {exception.InnerException?.Message ?? exception.Message}";
        }
        catch (System.Exception exception)
        {
            return $"Not available: {exception.Message}";
        }
    }
}
