using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Diagnostics;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class LogView
{
    private readonly StatusBarTextItem _statusBarItem = new() { Id = "LogView.Count", OrderIndex = -1 };

    private RadzenDataGrid<LogItemModel> _dataGrid = default!;
    private object? _logViewContext;
    private int _restoreTopRow = -1;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private IUserSettingsService<LogView> UserSettings { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    public override Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        Debug.Assert(Session is not null, "Log view is opening without session");
        if (Session?.DataStorage is not null)
        {
            _logViewContext = Session.DataStorage.OpenLogDataContext();
            Session.PropertiesChanged += OnPropertiesChanged;
            Session.Refreshing += OnRefreshing;
        }

        return base.OnOpeningAsync(cancellationToken);
    }

    public override async Task OnOpenedAsync(CancellationToken cancellationToken)
    {
        // Restore selected item.
        if (Session?.PropertyStorage is not null && Session?.DataStorage is not null && _logViewContext is not null)
        {
            if (Session.PropertyStorage.TryGetPropertyValue("SelectedLogItem.Id", out long selectedId) &&
                Session.PropertyStorage.TryGetPropertyValue("SelectedLogItem.Timestamp", out DateTimeOffset selectedTimestamp))
            {
                var item = await Session.DataStorage
                    .GetLogItemAndIndexAsync(selectedId, selectedTimestamp, false, _logViewContext, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

                if (item is not null)
                {
                    Session.SelectedLogItem = item.Value.item;
                }
            }

            if (Session.PropertyStorage.TryGetPropertyValue("DataGrid.TopRow", out int topRow))
            {
                _restoreTopRow = topRow;
            }

            if (Session.PropertyStorage.TryGetPropertyValue("DataGrid.LeftScrollPosition", out int leftScrollPosition))
            {
                await SetDataGridLeftScrollPositionAsync(leftScrollPosition, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            }
        }

        await base.OnOpenedAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
    }

    public override Task OnClosedAsync(CancellationToken cancellationToken)
    {
        if (Session?.DataStorage is not null && _logViewContext is not null)
        {
            Session.DataStorage.CloseLogDataContext(_logViewContext);
        }

        return base.OnClosedAsync(cancellationToken);
    }

    public override Task<(IEnumerable<Toolbar> toolbars, IEnumerable<KeyboardShortcut> shortcuts)> OnCreateToolbarsAsync(CancellationToken cancellationToken)
    {
        var gotoTimestampCommand = new AsyncCommand(OnGotoTimestamp);
        var gotoSelectedCommand = new AsyncCommand(OnGotoSelected);

        return Task.FromResult((
            (IEnumerable<Toolbar>)
            [
                new Toolbar
                {
                    Id = "LogView",
                    OrderIndex = 0,
                    Items =
                    [
                        new ToolbarButton
                        {
                            Icon = "edit",
                            Tooltip = "Edit session",
                            Command = new AsyncCommand(OnEditSession)
                        },
                        new ToolbarButton
                        {
                            Icon = "refresh",
                            Tooltip = "Refresh",
                            Command = new AsyncCommand(OnRefreshExecute)
                        },
                        new ToolbarButton
                        {
                            Icon = "schedule",
                            Tooltip = "Go to timestamp (Ctrl+G)",
                            Command = gotoTimestampCommand
                        },
                        new ToolbarButton
                        {
                            Icon = "arrow_selector_tool",
                            Tooltip = "Go to selected (Ctrl+S)",
                            Command = gotoSelectedCommand
                        }
                    ]
                }
            ],
            (IEnumerable<KeyboardShortcut>)
            [
                new KeyboardShortcut
                {
                    Id = "GotoTimestamp",
                    Command = gotoTimestampCommand,
                    CtrlKey = true,
                    KeyCode = "KeyG"
                },
                new KeyboardShortcut
                {
                    Id = "GotoSelected",
                    Command = gotoSelectedCommand,
                    CtrlKey = true,
                    KeyCode = "KeyS"
                }
            ]));
    }

    public override Task<IEnumerable<StatusBarItem>> OnCreateStatusBarItemsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<StatusBarItem>)
        [
            _statusBarItem
        ]);
    }

    public override async Task OnClosingAsync(CancellationToken cancellationToken)
    {
        if (Session is not null)
        {
            Session.PropertiesChanged -= OnPropertiesChanged;
            Session.Refreshing -= OnRefreshing;

            await Session.SaveDataAsync(CancellationToken.None).ConfigureAwait(true);

            // Persist selected log item:
            if (Session.SelectedLogItem is null)
            {
                Session.PropertyStorage?.RemovePropertyValue("SelectedLogItem.Id");
                Session.PropertyStorage?.RemovePropertyValue("SelectedLogItem.Timestamp");
            }
            else
            {
                Session.PropertyStorage?.SetPropertyValue("SelectedLogItem.Id", Session.SelectedLogItem.Id);
                Session.PropertyStorage?.SetPropertyValue("SelectedLogItem.Timestamp", Session.SelectedLogItem.LogEvent.Timestamp);
            }

            var topRow = await GetDataGridTopRowIndexAsync(cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            var leftScrollPosition = await GetDataGridLeftScrollPositionAsync(cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Session.PropertyStorage?.SetPropertyValue("DataGrid.TopRow", topRow);
            Session.PropertyStorage?.SetPropertyValue("DataGrid.LeftScrollPosition", leftScrollPosition);
        }

        await base.OnClosingAsync(cancellationToken).ConfigureAwait(true);
    }

    private IList<LogItemModel>? SelectedItems
    {
        get => Session?.SelectedLogItem is null ? null : [Session.SelectedLogItem];
        set
        {
            if (Session is null) return;
            Session.SelectedLogItem = value?.FirstOrDefault();
        }
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        InvokeAsync(_dataGrid.Reload);
    }

    private void OnUpdateStatusBarItem()
    {
        _statusBarItem.Text = $"Log event count: Total: {Model.Session?.FullLogEventCount} View: {Model.Session?.FilteredLogEventCount}";
    }

    private void OnPropertiesChanged(object? sender, EventArgs e)
    {
        InvokeAsync(_dataGrid.Reload);
    }

    private async Task LoadData(LoadDataArgs args)
    {
        if (Session is null || _logViewContext is null) return;

        try
        {
            await Session.LoadDataAsync(args, _logViewContext);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Update Data Grid", ex.Message);
        }

        _ = InvokeAsync(RestoreTopRow);

        OnUpdateStatusBarItem();
    }

    private async Task RestoreTopRow()
    {
        var topRow = _restoreTopRow;
        if (topRow >= 0)
        {
            if (await SetDataGridTopRowIndexAsync(topRow, CancellationToken.None)
                    .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext))
            {
                _restoreTopRow = -1;
            }
        }
    }

    private Task OnEditSession(object? _)
    {
        return ViewManager.NavigateToAsync(ViewId.SessionConfig, CancellationToken.None);
    }

    private Task OnRefreshExecute(object? _)
    {
        Session?.Refresh();
        return Task.CompletedTask;
    }

    private async Task OnGotoTimestamp(object? parameter)
    {
        if (Session?.DataStorage is null || _logViewContext is null) return;

        var timestamp = await GotoTimestampDialog
            .ShowDialog(DialogService, Session.SelectedLogItem?.LogEvent.Timestamp)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        if (!timestamp.HasValue) return;

        var tuple = await Session.DataStorage
            .GetLogItemAndIndexAsync(timestamp.Value, false, _logViewContext, CancellationToken.None)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        if (tuple is null) return;

        await _dataGrid.SelectRow(tuple.Value.item).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        await ScrollDataGridRowIntoViewAsync(tuple.Value.index, CancellationToken.None)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
    }

    private async Task OnGotoSelected(object? parameter)
    {
        if (Session?.DataStorage is null || _logViewContext is null || Session?.SelectedLogItem is null) return;

        var tuple = await Session.DataStorage
            .GetLogItemAndIndexAsync(Session.SelectedLogItem, true, _logViewContext, CancellationToken.None)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        if (tuple is null) return;

        await ScrollDataGridRowIntoViewAsync(tuple.Value.index, CancellationToken.None)
            .ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
    }

    private static RenderFragment<LogPropertyModel> RenderPropertyColumn => property => builder =>
    {
        var (type, propertyPrefix) = property.PropertyTypes.Count == 1
            ? property.PropertyTypes.FirstOrDefault() switch
            {
                PropertyType.String => (typeof(string), "s:"),
                PropertyType.Float => (typeof(double?), "f:"),
                PropertyType.Integer => (typeof(long?), "i:"),
                PropertyType.Unsigned => (typeof(ulong?), "u:"),
                /* For now only support string filtering for complex properties
                PropertyType.List => (typeof(IReadOnlyList<LogEventPropertyValue>), "l:"),
                PropertyType.Map => (typeof(IReadOnlyDictionary<ScalarValue, LogEventPropertyValue>), "m:"),
                PropertyType.Object => (typeof(LogEventPropertyValue), "o:"), */
                _ => (typeof(string), "")
            } : (typeof(string), "");

        builder.OpenComponent<RadzenDataGridColumn<LogItemModel>>(1);
        builder.AddAttribute(2, "Title", property.Name);
        builder.AddAttribute(3, "Type", type);
        builder.AddAttribute(4, "Width", "100px");
        builder.AddAttribute(5, "MinWidth", "40px");
        builder.AddAttribute(6, "Property", PropertyAccess.GetDynamicPropertyExpression(propertyPrefix + property.Name, type));

        builder.AddAttribute(7, "Template",
            (RenderFragment<LogItemModel>)(item => builder2 =>
            {
                builder2.AddContent(8, item[property.Name]);
            }));

        builder.CloseComponent();

        //<RadzenDataGridColumn Title="@property.Name" Type="@type" Width="100px" MinWidth="40px"
        //                      Property="@PropertyAccess.GetDynamicPropertyExpression(propertyPrefix + property.Name, type)">
        //    <Template Context="item">
        //        @item[property.Name]
        //    </Template>
        //</RadzenDataGridColumn>
    };
}