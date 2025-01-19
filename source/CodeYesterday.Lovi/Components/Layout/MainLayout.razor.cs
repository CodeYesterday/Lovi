using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Windows.Input;
using ToolbarItem = CodeYesterday.Lovi.Input.ToolbarItem;

namespace CodeYesterday.Lovi.Components.Layout;

public partial class MainLayout
{
    private readonly List<ToolbarItem> _hookedItems = new();
    private readonly List<ICommand> _hookedCommands = new();

    [Inject]
    private DialogService DialogService { get; set; } = default!;

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