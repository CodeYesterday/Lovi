using CodeYesterday.Lovi.Components.Pages;
using CodeYesterday.Lovi.Components.Panes;
using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Services;

internal class ViewManager : IViewManagerInternal
{
    private readonly ISessionService _sessionService;
    private readonly List<LoviPane> _panes = new();

    private NavigationManager? _navigationManager;
    private LoviView? _lastView;
    private List<LoviPane>? _lastPanes;
    private Toolbar[]? _toolbars;
    private KeyboardShortcut[]? _shortcuts;
    private StatusBarItem[]? _statusBarItems;

    public LoviView? CurrentView { get; internal set; }

    public ViewModel? SelectedView => GetSelectedView();

    public IList<ViewModel> Views { get; } = new List<ViewModel>();

    public event EventHandler? ViewsChanged;

    public ViewManager(ISessionService sessionService)
    {
        _sessionService = sessionService;
        Views.Add(new() { Type = ViewType.StartView });
    }

    public ViewModel? GetView(ViewType type, int sessionId = 0)
    {
        return Views.FirstOrDefault(v => v.Type == type && v.SessionId == sessionId);
    }

    public Task ShowViewAsync(ViewModel view, bool closeCurrentView, CancellationToken cancellationToken)
    {
        CheckInitialized();

        if (!Views.Contains(view)) return Task.CompletedTask;
        if (view.Equals(SelectedView)) return Task.CompletedTask;

        var currentView = SelectedView;
        if (closeCurrentView && currentView is not null)
        {
            Views.Remove(currentView);

            ViewsChanged?.Invoke(this, EventArgs.Empty);
        }

        _navigationManager.NavigateTo(view.Uri);

        return Task.CompletedTask;
    }

    public Task AddViewAsync(ViewModel view, bool show, bool closeCurrentView, CancellationToken cancellationToken)
    {
        CheckInitialized();

        Views.Add(view);

        ViewsChanged?.Invoke(this, EventArgs.Empty);

        if (show)
        {
            return ShowViewAsync(view, closeCurrentView, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public async Task CloseViewAsync(ViewModel view, CancellationToken cancellationToken)
    {
        var currentView = SelectedView;
        Views.Remove(view);

        ViewsChanged?.Invoke(this, EventArgs.Empty);

        if (view.IsSessionView && !Views.Any(v => v.IsSessionView && v.SessionId == view.SessionId))
        {
            await _sessionService.CloseSessionAsync(view.SessionId)
                .ConfigureAwait(true);
        }

        if (view.Equals(currentView))
        {
            var newView =
                (view.SessionId > 0 ? Views.FirstOrDefault(v => v.SessionId == view.SessionId) : null) ??
                ((short)view.Type < 0 ? Views.MaxBy(v => v.SortOrder) : null) ??
                Views.FirstOrDefault();

            if (newView is not null)
            {
                await ShowViewAsync(newView, false, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            }
        }
    }

    public Task Refresh(CancellationToken cancellationToken)
    {
        CheckInitialized();

        // reset the navigation manager, since we will get a new one.
        var navigationManager = _navigationManager;

        var uri = GetCurrentLocalUri();
        navigationManager.NavigateTo(uri, true);

        return Task.CompletedTask;
    }

    private ViewModel? GetSelectedView()
    {
        var uri = GetCurrentLocalUri();
        return Views.FirstOrDefault(v => string.Equals(v.Uri, uri, StringComparison.OrdinalIgnoreCase));
    }

    private string GetCurrentLocalUri()
    {
        if (_navigationManager is null) return string.Empty;
        return "/" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
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

        if (view.InputHandler is not null)
        {
            var (t, s) = await view
                .OnCreateToolbarsAsync(cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            _toolbars = t.ToArray();
            foreach (var toolbar in _toolbars)
            {
                view.InputHandler.AddOrUpdateToolbar(toolbar);
            }

            _shortcuts = s.ToArray();
            foreach (var shortcut in _shortcuts)
            {
                view.InputHandler.AddOrUpdateShortcut(shortcut);
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

    public Task OnOpeningPaneAsync(LoviPane pane)
    {
        _panes.Add(pane);
        return pane.OnOpeningAsync(CancellationToken.None);
    }

    public Task OnPaneOpenedAsync(LoviPane pane)
    {
        Debug.Assert(_panes.Contains(pane), "PaneOpened called but OpeningPane was not");

        return pane.OnOpenedAsync(CancellationToken.None);
    }

    public async Task OnAppExitAsync(CancellationToken cancellationToken)
    {
        // Notify current view about being closed.
        _lastView = CurrentView;
        if (_lastView is not null)
        {
            await _lastView.OnClosingAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            CurrentView = null;
            await _lastView.OnClosedAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            _lastView = null;
        }
    }

    [MemberNotNull(nameof(_navigationManager))]
    private void CheckInitialized()
    {
        if (_navigationManager is null) throw new InvalidOperationException("ViewManager was not initialized");
    }

    private async ValueTask OnLocationChanging(LocationChangingContext arg)
    {
        _lastPanes = [.. _panes];
        foreach (var pane in _lastPanes)
        {
            await pane.OnClosingAsync(CancellationToken.None).ConfigureAwait(true);
        }
        _panes.Clear();

        _lastView = CurrentView;
        if (_lastView is not null)
        {
            await _lastView.OnClosingAsync(CancellationToken.None).ConfigureAwait(true);
        }

        CurrentView = null;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        OnViewClosed();
    }

    private void OnViewClosed()
    {
        if (_lastPanes is not null)
        {
            foreach (var pane in _lastPanes)
            {
                _ = pane.OnClosedAsync(CancellationToken.None);
            }

            _lastPanes = null;
        }

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

            if (_lastView.InputHandler is not null)
            {
                if (_toolbars is not null)
                {
                    foreach (var toolbar in _toolbars)
                    {
                        _lastView.InputHandler.RemoveToolbar(toolbar.Id);
                    }

                    _toolbars = null;
                }
                if (_shortcuts is not null)
                {
                    foreach (var shortcut in _shortcuts)
                    {
                        _lastView.InputHandler.RemoveShortcut(shortcut.Id);
                    }

                    _shortcuts = null;
                }
            }

            _ = _lastView.OnClosedAsync(CancellationToken.None);
            _lastView = null;
        }
    }
}
