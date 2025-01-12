namespace CodeYesterday.Lovi.Models;

public abstract class StatusBarItem
{
    private bool _isVisible = true;
    private string? _toolTip;

    public required string Id { get; init; }

    public required int OrderIndex { get; init; }

    public string? Tooltip
    {
        get => _toolTip;
        set
        {
            if (string.Equals(value, _toolTip)) return;
            _toolTip = value;
            OnStateHasChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value == _isVisible) return;
            _isVisible = value;
            OnStateHasChanged();
        }
    }

    public event EventHandler? StateHasChanged;

    public event EventHandler? Removed;

    internal virtual void OnRemoved()
    {
        Removed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnStateHasChanged()
    {
        StateHasChanged?.Invoke(this, EventArgs.Empty);
    }
}
