using CodeYesterday.Lovi.Components;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Services;

internal class ViewManager : IViewManagerInternal
{
    private static readonly Dictionary<ViewId, string> Views = new Dictionary<ViewId, string>()
    {
        { ViewId.StartView, "/" },
        { ViewId.SessionConfig, "/session_config" },
        { ViewId.LogView, "/log" }
    };

    private NavigationManager? _navigationManager;

    private LoviView? _lastView;
    private Toolbar[]? _toolbars;
    private StatusBarItem[]? _statusBarItems;

    public LoviView? CurrentView { get; private set; }

    public Task ShowViewAsync(ViewId id, CancellationToken cancellationToken)
    {
        CheckInitialized();

        if (!Views.TryGetValue(id, out var uri)) throw new ArgumentOutOfRangeException(nameof(id), id, null);

        _navigationManager.NavigateTo(uri);

        return Task.CompletedTask;
    }

    public void SetNavigationManager(NavigationManager navigationManager)
    {
        if (_navigationManager is not null) return;

        _navigationManager = navigationManager;

        _navigationManager.RegisterLocationChangingHandler(OnLocationChanging);

        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public async Task OnOpeningViewAsync(LoviView view)
    {
        ArgumentNullException.ThrowIfNull(view, nameof(view));

        Debug.Assert(CurrentView is null, "CurrentView already set in OnOpeningViewAsync");

        var cancellationToken = CancellationToken.None;

        // If last view is set call OnClosedAsync here already.
        // OnLocationChanged is after this call.
        OnViewClosed();

        CurrentView = view;

        await view.OnOpeningAsync(cancellationToken).ConfigureAwait(true);

        if (view.ToolbarContainer is not null)
        {
            _toolbars = (await view.OnCreateToolbarsAsync(cancellationToken)).ToArray();
            foreach (var toolbar in _toolbars)
            {
                view.ToolbarContainer.AddOrUpdateToolbar(toolbar);
            }
        }

        if (view.StatusBarModel is not null)
        {
            _statusBarItems = (await view.OnCreateStatusBarItemsAsync(cancellationToken)).ToArray();
            foreach (var item in _statusBarItems)
            {
                view.StatusBarModel.AddOrUpdateItem(item);
            }
        }

        if (view.PaneLayout is not null)
        {
            await view.OnCreatePanesAsync(view.PaneLayout, cancellationToken).ConfigureAwait(true);

            view.PaneLayout.PanesChanged();
        }
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
        OnViewClosed();
    }

    private void OnViewClosed()
    {
        if (_lastView is not null)
        {
            if (_lastView.PaneLayout is not null)
            {
                _lastView.PaneLayout.PaneUserSettings = null;

                _lastView.PaneLayout.LeftPane = null;
                _lastView.PaneLayout.RightPane = null;
                _lastView.PaneLayout.TopPane = null;
                _lastView.PaneLayout.BottomPane = null;

                _lastView.PaneLayout.PanesChanged();
            }

            if (_lastView.StatusBarModel is not null && _statusBarItems is not null)
            {
                foreach (var item in _statusBarItems)
                {
                    _lastView.StatusBarModel.RemoveItem(item.Id);
                }

                _statusBarItems = null;
            }

            if (_lastView.ToolbarContainer is not null && _toolbars is not null)
            {
                foreach (var toolbar in _toolbars)
                {
                    _lastView.ToolbarContainer.RemoveToolbar(toolbar.Id);
                }

                _toolbars = null;
            }

            _ = _lastView.OnClosedAsync(CancellationToken.None);
            _lastView = null;
        }
    }
}
