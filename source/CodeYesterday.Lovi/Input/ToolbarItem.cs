namespace CodeYesterday.Lovi.Input;

public abstract class ToolbarItem
{
    private string? _icon;
    private string? _title;
    private bool _isVisible = true;
    private string? _toolTip;

    public string? Icon
    {
        get => _icon;
        set
        {
            if (string.Equals(value, _icon)) return;
            _icon = value;
            OnStateHasChanged();
        }
    }

    public string? Title
    {
        get => _title;
        set
        {
            if (string.Equals(value, _title)) return;
            _title = value;
            OnStateHasChanged();
        }
    }

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

    protected virtual void OnStateHasChanged()
    {
        StateHasChanged?.Invoke(this, EventArgs.Empty);
    }
}
