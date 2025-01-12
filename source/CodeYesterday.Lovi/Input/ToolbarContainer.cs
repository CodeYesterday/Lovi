namespace CodeYesterday.Lovi.Input;

public class ToolbarContainer
{
    private readonly List<Toolbar> _toolbars = new();

    public IReadOnlyList<Toolbar> Toolbars { get; }

    public event EventHandler? ToolbarsChanged;

    public ToolbarContainer()
    {
        Toolbars = _toolbars.AsReadOnly();
    }

    public Toolbar? GetToolbar(string id) => _toolbars.FirstOrDefault(tb => tb.Id.Equals(id, StringComparison.Ordinal));

    public void RemoveToolbar(string id)
    {
        var toolbar = GetToolbar(id);
        if (toolbar != null)
        {
            _toolbars.Remove(toolbar);
            toolbar.OnRemoved();
            ToolbarsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddOrUpdateToolbar(Toolbar toolbar)
    {
        var oldToolbar = GetToolbar(toolbar.Id);
        if (!ReferenceEquals(oldToolbar, toolbar))
        {
            if (oldToolbar != null)
            {
                _toolbars.Remove(oldToolbar);
                oldToolbar.OnRemoved();
            }
            _toolbars.Add(toolbar);
        }

        ToolbarsChanged?.Invoke(this, EventArgs.Empty);
    }
}
