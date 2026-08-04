using System.ComponentModel;
using System.Windows.Forms;

namespace Fs.Fox.CAD.Diagnostics.AutoCad;

/// <summary>
/// Preserves the legacy MgdDbg context-menu model across .NET Framework and
/// modern Windows Forms. AutoCAD 2025 targets .NET 8, where ContextMenu and
/// MenuItem are no longer available.
/// </summary>
#if AC_2025
internal sealed class LegacyContextMenu : ContextMenuStrip
{
    private bool _separatorsNormalized;

    public ToolStripItemCollection MenuItems => Items;

    public event EventHandler Popup;

    protected override void OnOpening(CancelEventArgs e)
    {
        NormalizeLegacySeparators();
        base.OnOpening(e);
        Popup?.Invoke(this, EventArgs.Empty);
    }

    private void NormalizeLegacySeparators()
    {
        if (_separatorsNormalized)
            return;

        // The old ContextMenu interpreted a MenuItem whose text was "-" as a
        // separator. ContextMenuStrip does not, so translate it just before the
        // first display while preserving the designer-defined item order.
        for (var index = 0; index < Items.Count; index++)
        {
            if (Items[index] is not LegacyMenuItem item || item.Text != "-")
                continue;

            Items.RemoveAt(index);
            Items.Insert(index, new ToolStripSeparator());
        }

        _separatorsNormalized = true;
    }
}

internal class LegacyMenuItem : ToolStripMenuItem
{
    public LegacyMenuItem()
    {
    }

    public LegacyMenuItem(string text)
        : base(text)
    {
    }

    // The old WinForms designer persisted Index. ToolStrip uses collection
    // order instead, so this compatibility property is intentionally inert.
    public int Index { get; set; }
}
#else
internal sealed class LegacyContextMenu : System.Windows.Forms.ContextMenu
{
}

internal class LegacyMenuItem : System.Windows.Forms.MenuItem
{
    public LegacyMenuItem()
    {
    }

    public LegacyMenuItem(string text)
        : base(text)
    {
    }
}
#endif

internal static class LegacyMenuAdapter
{
    internal static void Attach(Control control, LegacyContextMenu menu)
    {
        if (control is null)
            throw new ArgumentNullException(nameof(control));
        if (menu is null)
            throw new ArgumentNullException(nameof(menu));
#if AC_2025
        control.ContextMenuStrip = menu;
#else
        control.ContextMenu = menu;
#endif
    }
}
