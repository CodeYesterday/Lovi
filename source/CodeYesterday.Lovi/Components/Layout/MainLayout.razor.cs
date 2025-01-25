using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using ToolbarItem = CodeYesterday.Lovi.Input.ToolbarItem;

namespace CodeYesterday.Lovi.Components.Layout;

public partial class MainLayout : IAsyncDisposable
{
    private readonly List<ToolbarItem> _hookedItems = new();
    private readonly List<ICommand> _hookedCommands = new();

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MainLayout>? _objRef;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Inject]
    private IViewManagerInternal ViewManagerInternal { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    private InputHandler InputHandler { get; } = new();

    private StatusBarModel StatusBar { get; } = new();

    public async ValueTask DisposeAsync()
    {
        if (_objRef is not null)
        {
            try
            {
                _objRef.Dispose();
            }
            catch
            {
                // ignored
            }
        }
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch
            {
                // ignored
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ViewManagerInternal.SetNavigationManager(NavigationManager);

        await EnsureModuleLoadedAsync(CancellationToken.None)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        // Main toolbar
        InputHandler.AddOrUpdateToolbar(new()
        {
            Id = "~Main",
            OrderIndex = 0,
            Items =
            [
                new ToolbarButton
                {
                    Icon = "home",
                    Tooltip = "Home",
                    Command = new AsyncCommand(OnGoHome)
                }
            ]
        });

        // System toolbar
        InputHandler.AddOrUpdateToolbar(new()
        {
            Id = "~System",
            OrderIndex = -1,
            Items =
                [
                    new ToolbarMenuButton
                    {
                        Icon = "settings",
                        Tooltip = "Settings",
                        Items =
                            [
                                new()
                                {
                                    Icon = "settings",
                                    Title = "Settings",
                                    Command = new AsyncCommand(OnShowSettings)
                                },
                                new()
                                {
                                    Icon = "help",
                                    Title = "Help",
                                    Command = new AsyncCommand(OnShowHelp)
                                },
                                new()
                                {
                                    Icon = "info",
                                    Title = "About",
                                    Command = new AsyncCommand(OnShowAbout)
                                }
                            ]
                    }
                ]
        });

        InputHandler.ToolbarsChanged += OnToolbarsChanged;
    }

    private async Task OnGoHome(object? parameter)
    {
        if (Model.Session is not null)
        {
            try
            {
                await Model.Session.UnloadDataAsync(CancellationToken.None);
            }
            catch
            {
                // ignored
            }

            Model.Session = null;
        }

        await ViewManagerInternal.NavigateToAsync(ViewId.StartView, CancellationToken.None).ConfigureAwait(true);
    }

    private Task OnShowSettings(object? parameter)
    {
        return ViewManagerInternal.NavigateToAsync(ViewId.Settings, CancellationToken.None);
    }

    private Task OnShowHelp(object? parameter)
    {
        return Task.CompletedTask;
    }

    private Task OnShowAbout(object? parameter)
    {
        return AboutDialog.ShowAsync(DialogService);
    }

    private void OnToolbarsChanged(object? sender, EventArgs e)
    {
        // TODO: StateHasChanged should be done on the AppToolbar/ToolbarControl level.
        foreach (var item in _hookedItems)
        {
            item.StateHasChanged -= OnStateHasChanged;
        }
        _hookedItems.Clear();

        foreach (var command in _hookedCommands)
        {
            command.CanExecuteChanged -= OnStateHasChanged;
        }
        _hookedCommands.Clear();

        foreach (var toolBar in InputHandler.Toolbars)
        {
            foreach (var item in toolBar.Items)
            {
                item.StateHasChanged += OnStateHasChanged;
                _hookedItems.Add(item);

                if (item is ToolbarButton button)
                {
                    button.Command.CanExecuteChanged += OnStateHasChanged;
                    _hookedCommands.Add(button.Command);
                }
            }
        }

        InvokeAsync(StateHasChanged);
    }

    private void OnStateHasChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    [MemberNotNull(nameof(_jsModule))]
    [MemberNotNull(nameof(_objRef))]
    private async Task EnsureModuleLoadedAsync(CancellationToken cancellationToken)
    {
        if (_jsModule is not null && _objRef is not null) return;

        // Required to suppress CS8774 in async method
        _jsModule = null!;
        _objRef = null!;

        string jsFilePath = $"./Components/Layout/{nameof(MainLayout)}.razor.js";
        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, jsFilePath);

        _objRef = DotNetObjectReference.Create(this);
        await _jsModule.InvokeVoidAsync("addKeyDownListener", cancellationToken, _objRef).ConfigureAwait(true);
    }

    [JSInvokable]
    public void OnKeyEvent(string code, bool shiftKey, bool altKey, bool ctrlKey)
    {
        InputHandler.OnKeyPress(code, shiftKey, altKey, ctrlKey);
    }
}