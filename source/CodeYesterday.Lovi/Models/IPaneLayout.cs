using CodeYesterday.Lovi.Services;

namespace CodeYesterday.Lovi.Models;

/// <summary>
/// Interface to provide the pane layout content.
/// </summary>
[PublicAPI]
public interface IPaneLayout
{
    /// <summary>
    /// Gets or sets the user-settings to be used for persisting the visibility and size of the panes.
    /// </summary>
    IUserSettingsService? PaneUserSettings { get; set; }

    /// <summary>
    /// Gets or sets the information for the left pane. Set to <see langword="null"/> if the left pane is not used.
    /// </summary>
    PaneItem? LeftPane { get; set; }

    /// <summary>
    /// Gets or sets the information for the right pane. Set to <see langword="null"/> if the right pane is not used.
    /// </summary>
    PaneItem? RightPane { get; set; }

    /// <summary>
    /// Gets or sets the information for the top pane. Set to <see langword="null"/> if the top pane is not used.
    /// </summary>
    PaneItem? TopPane { get; set; }

    /// <summary>
    /// Gets or sets the information for the bottom pane. Set to <see langword="null"/> if the bottom pane is not used.
    /// </summary>
    PaneItem? BottomPane { get; set; }

    /// <summary>
    /// Updates the pane content and the pane view toolbar.
    /// </summary>
    /// <remarks>
    /// MUST be called by the view after the <see cref="PaneItem"/>s have been set.
    /// </remarks>
    void PanesChanged();
}