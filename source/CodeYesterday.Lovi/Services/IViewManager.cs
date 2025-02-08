using CodeYesterday.Lovi.Components.Pages;
using CodeYesterday.Lovi.Components.Panes;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Services;

/// <summary>
/// Interface for the Lovi view manager.
/// </summary>
[PublicAPI]
public interface IViewManager
{
    /// <summary>
    /// Gets the current view.
    /// </summary>
    LoviView? CurrentView { get; }

    public ViewModel? SelectedView { get; }

    IList<ViewModel> Views { get; }

    event EventHandler? ViewsChanged;

    ViewModel? GetView(ViewType type, int sessionId = 0);

    /// <summary>
    /// Navigates to the view.
    /// </summary>
    /// <param name="view">The view to navigate to.</param>
    /// <param name="closeCurrentView"><see langword="true"/> to close the currently opened view.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns></returns>
    Task ShowViewAsync(ViewModel view, bool closeCurrentView, CancellationToken cancellationToken);

    Task AddViewAsync(ViewModel view, bool show, bool closeCurrentView, CancellationToken cancellationToken);

    Task CloseViewAsync(ViewModel view, CancellationToken cancellationToken);

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

public enum ViewType : short
{
    StartView = 0,
    SessionConfig = 100,
    LogView = 101,
    Settings = -1
}
