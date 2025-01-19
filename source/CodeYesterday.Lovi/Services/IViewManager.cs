using CodeYesterday.Lovi.Components;
using CodeYesterday.Lovi.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Services;

[PublicAPI]
public interface IViewManager
{
    LoviView? CurrentView { get; }

    Task ShowViewAsync(ViewId id, CancellationToken cancellationToken);
}


[PublicAPI]
public interface IViewManagerInternal : IViewManager
{
    void SetNavigationManager(NavigationManager navigationManager);

    Task OnOpeningViewAsync(LoviView view);

    Task OnViewOpenedAsync(LoviView view);

    Task OnOpeningPaneAsync(LoviPane pane);

    Task OnPaneOpenedAsync(LoviPane pane);
}

public enum ViewId
{
    StartView,
    SessionConfig,
    LogView
}
