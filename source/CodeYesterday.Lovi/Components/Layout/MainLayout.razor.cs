using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using System.Windows.Input;
using ToolbarItem = CodeYesterday.Lovi.Input.ToolbarItem;

namespace CodeYesterday.Lovi.Components.Layout;

public partial class MainLayout
{
    private readonly List<ToolbarItem> _hookedItems = new();
    private readonly List<ICommand> _hookedCommands = new();

    [Inject]
    private IViewManagerInternal ViewManagerInternal { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    private ToolbarContainer ToolbarContainer { get; } = new();

    private StatusBarModel StatusBar { get; } = new();

    protected override void OnInitialized()
    {
        ViewManagerInternal.SetNavigationManager(NavigationManager);

        NavigationManager.RegisterLocationChangingHandler(_ =>
        {
            foreach (var toolbar in ToolbarContainer.Toolbars.ToArray())
            {
                if (!toolbar.Id.StartsWith("~"))
                {
                    ToolbarContainer.RemoveToolbar(toolbar.Id);
                }
            }

            foreach (var item in StatusBar.Items.ToArray())
            {
                if (!item.Id.StartsWith("~"))
                {
                    StatusBar.RemoveItem(item.Id);
                }
            }

            return ValueTask.CompletedTask;
        });

        // Main toolbar
        ToolbarContainer.AddOrUpdateToolbar(new()
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
        ToolbarContainer.AddOrUpdateToolbar(new()
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

        ToolbarContainer.ToolbarsChanged += OnToolbarsChanged;
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

        await ViewManagerInternal.ShowViewAsync(ViewId.StartView, CancellationToken.None).ConfigureAwait(true);
    }

    private Task OnShowSettings(object? parameter)
    {
        return Task.CompletedTask;
    }

    private Task OnShowHelp(object? parameter)
    {
        return Task.CompletedTask;
    }

    private Task OnShowAbout(object? parameter)
    {
        return Task.CompletedTask;
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

        foreach (var toolBar in ToolbarContainer.Toolbars)
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
}