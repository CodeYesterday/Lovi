namespace CodeYesterday.Lovi.Input;

public class Toolbar
{
    public required string Id { get; init; }

    public int OrderIndex { get; init; }

    public IList<ToolbarItem> Items { get; init; } = [];

    public event EventHandler? Removed;

    internal void OnRemoved()
    {
        Removed?.Invoke(this, EventArgs.Empty);
    }
}
