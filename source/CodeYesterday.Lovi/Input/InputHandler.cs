namespace CodeYesterday.Lovi.Input;

public class InputHandler
{
    private readonly List<Toolbar> _toolbars = new();
    private readonly List<KeyboardShortcut> _shortcuts = new();

    public IReadOnlyList<Toolbar> Toolbars { get; }

    public IReadOnlyList<KeyboardShortcut> Shortcuts { get; }

    public event EventHandler? ToolbarsChanged;

    public InputHandler()
    {
        Toolbars = _toolbars.AsReadOnly();
        Shortcuts = _shortcuts.AsReadOnly();
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

    public KeyboardShortcut? GetShortcut(string id) => _shortcuts.FirstOrDefault(s => s.Id.Equals(id, StringComparison.Ordinal));

    public void RemoveShortcut(string id)
    {
        var shortcut = GetShortcut(id);
        if (shortcut != null)
        {
            _shortcuts.Remove(shortcut);
        }
    }

    public void AddOrUpdateShortcut(KeyboardShortcut shortcut)
    {
        var oldShortcut = GetShortcut(shortcut.Id);
        if (!ReferenceEquals(oldShortcut, shortcut))
        {
            if (oldShortcut != null)
            {
                _shortcuts.Remove(oldShortcut);
            }
            _shortcuts.Add(shortcut);
        }
    }

    internal Task OnKeyPress(string code, bool shiftKey, bool altKey, bool ctrlKey)
    {
        var shortcut = _shortcuts.FirstOrDefault(s => string.Equals(s.KeyCode, code, StringComparison.Ordinal) && s.ShiftKey == shiftKey && s.AltKey == altKey && s.CtrlKey == ctrlKey);
        if (shortcut is null) return Task.CompletedTask;

        if (shortcut.Command.CanExecute(shortcut.CommandParameter))
        {
            // Fire and forget
            shortcut.Command.Execute(shortcut.CommandParameter);
        }
        return Task.CompletedTask;
    }
}
