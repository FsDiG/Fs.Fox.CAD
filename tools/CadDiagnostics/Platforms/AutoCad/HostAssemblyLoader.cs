using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Supplies already loaded runtime assemblies to the legacy class browser.
/// Reflection-only loading was removed from .NET 8, and the old implementation
/// also depended on hard-coded .NET 2.0 and AEC development paths.
/// </summary>
internal static class HostAssemblyLoader
{
    internal static Assembly[] GetClassBrowserAssemblies()
    {
        var assemblies = new List<Assembly>
        {
            typeof(object).Assembly,
            typeof(Document).Assembly,
            typeof(DBObject).Assembly,
            typeof(RXObject).Assembly,
        };

        // Preserve optional AEC visibility when those products have already
        // loaded their managed assemblies, without probing machine-specific paths.
        assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
        {
            var name = assembly.GetName().Name;
            return string.Equals(name, "AecBaseMgd", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(name, "AecArchMgd", StringComparison.OrdinalIgnoreCase);
        }));

        return assemblies
            .GroupBy(assembly => assembly.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }
}
