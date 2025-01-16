using CodeYesterday.Lovi.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CodeYesterday.Lovi.Services;

internal class ViewManager : IViewManagerInternal
{
    private NavigationManager? _navigationManager;

    private LoviView? _lastView;

    public LoviView? CurrentView { get; private set; }

    public Task ShowViewAsync(ViewId id, CancellationToken cancellationToken)
    {
        CheckInitialized();

        switch (id)
        {
            case ViewId.StartView:
                _navigationManager.NavigateTo("/");
                break;

            case ViewId.SessionConfig:
                _navigationManager.NavigateTo("/session_config");
                break;

            case ViewId.LogView:
                _navigationManager.NavigateTo("/log");

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }

        return Task.CompletedTask;
    }

    public void SetNavigationManager(NavigationManager navigationManager)
    {
        if (_navigationManager is not null) return;

        _navigationManager = navigationManager;

        _navigationManager.RegisterLocationChangingHandler(OnLocationChanging);

        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public Task OnOpeningViewAsync(LoviView view)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));

        Debug.Assert(CurrentView is null, "CurrentView already set in OnOpeningViewAsync");

        CurrentView = view;

        return view.OnOpeningAsync(CancellationToken.None);
    }

    public Task OnViewOpenedAsync(LoviView view)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));

        Debug.Assert(ReferenceEquals(view, CurrentView), "view != CurrentView in in OnViewOpenedAsync");

        return view.OnOpenedAsync(CancellationToken.None);
    }

    [MemberNotNull(nameof(_navigationManager))]
    private void CheckInitialized()
    {
        if (_navigationManager is null) throw new InvalidOperationException("ViewManager was not initialized");
    }

    private ValueTask OnLocationChanging(LocationChangingContext arg)
    {
        _lastView = CurrentView;
        if (_lastView is not null)
        {
            _ = _lastView.OnClosingAsync(CancellationToken.None);
        }

        CurrentView = null;

        return ValueTask.CompletedTask;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (_lastView is not null)
        {
            _ = _lastView.OnClosedAsync(CancellationToken.None);
            _lastView = null;
        }
    }
}
