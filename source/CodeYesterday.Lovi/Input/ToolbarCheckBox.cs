using CodeYesterday.Lovi.Models;
using Radzen;

namespace CodeYesterday.Lovi.Input;

public class ToolbarCheckBox : ToolbarItem
{
    private bool _isChecked;
    private bool _isDisabled;
    private string? _isCheckedIcon;
    private ButtonStyle _isCheckedButtonStyle = ButtonStyle.Secondary;
    private string? _isCheckedTooltip;

    public ButtonSize? ButtonSize { get; init; }

    public Variant? ButtonVariant { get; init; }

    public string? IsCheckedIcon
    {
        get => _isCheckedIcon;
        set
        {
            if (string.Equals(value, _isCheckedIcon)) return;
            _isCheckedIcon = value;
            OnStateHasChanged();
        }
    }

    public ButtonStyle IsCheckedButtonStyle
    {
        get => _isCheckedButtonStyle;
        set
        {
            if (value == _isCheckedButtonStyle) return;
            _isCheckedButtonStyle = value;
            OnStateHasChanged();
        }
    }

    public string? IsCheckedTooltip
    {
        get => _isCheckedTooltip;
        set
        {
            if (string.Equals(value, _isCheckedTooltip)) return;
            _isCheckedTooltip = value;
            OnStateHasChanged();
        }
    }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (value == _isChecked) return;
            _isChecked = value;
            OnStateHasChanged();
            IsCheckedChanged?.Invoke(this, new(!value, value));
        }
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            if (value == _isDisabled) return;
            _isDisabled = value;
            OnStateHasChanged();
        }
    }

    public event EventHandler<ChangedEventArgs<bool>>? IsCheckedChanged;
}
