using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Models;

/// <summary>
/// A pane item in the pane layout.
/// </summary>
[PublicAPI]
public class PaneItem
{
    private bool _isVisible = true;

    /// <summary>
    /// Gets or sets the <see cref="RenderFragment"/> of the content.
    /// </summary>
    public required RenderFragment Content { get; set; }

    /// <summary>
    /// Gets or sets the custom icon for the visibility toggle button in the toolbar.
    /// </summary>
    public string? ToggleViewIcon { get; set; }

    /// <summary>
    /// Gets or sets if the pane is visible.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value == _isVisible) return;
            _isVisible = value;
            VisibilityChanged?.Invoke(this, new ChangedEventArgs<bool>(!value, value));
        }
    }

    /// <summary>
    /// Is raised when the visibility of the pane has changed.
    /// </summary>
    public event EventHandler<ChangedEventArgs<bool>>? VisibilityChanged;
}