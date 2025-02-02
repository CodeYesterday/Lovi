using CodeYesterday.Lovi.Components;
using CodeYesterday.Lovi.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Services;

/// <summary>
/// Interface for the Lovi view manager.
/// </summary>
[PublicAPI]
public interface IViewManager
{
    /// <summary>
    /// Gets the previous view ID.
    /// </summary>
    public ViewId? PreviousViewId { get; }

    /// <summary>
    /// Gets the current view ID.
    /// </summary>
    public ViewId CurrentViewId { get; }

    /// <summary>
    /// Gets the current view.
    /// </summary>
    LoviView? CurrentView { get; }

    /// <summary>
    /// Navigates to the view.
    /// </summary>
    /// <param name="id">The view to navigate to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns></returns>
    Task NavigateToAsync(ViewId id, CancellationToken cancellationToken);

    /// <summary>
    /// Navigates to the previous view. If there is no previous view nothing happens.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns <see langword="true"/> if there is a previous view, otherwise <see langword="false"/>.</returns>
    Task<bool> NavigateBackAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the view.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Refresh(CancellationToken cancellationToken);
}

[PublicAPI]
public interface IViewManagerInternal : IViewManager
{
    void SetNavigationManager(NavigationManager navigationManager);

    Task OnOpeningViewAsync(LoviView view);

    Task OnViewOpenedAsync(LoviView view);

    Task OnOpeningPaneAsync(LoviPane pane);

    Task OnPaneOpenedAsync(LoviPane pane);

    Task OnAppExitAsync(CancellationToken cancellationToken);
}

public enum ViewId
{
    StartView,
    SessionConfig,
    LogView,
    Settings
}
