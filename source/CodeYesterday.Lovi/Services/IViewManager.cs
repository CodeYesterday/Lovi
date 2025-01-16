using CodeYesterday.Lovi.Components;
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
}

public enum ViewId
{
    StartView,
    SessionConfig,
    LogView
}
