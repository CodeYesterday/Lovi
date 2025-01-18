using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Serilog.Events;
using System.Diagnostics;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class LogView
{
    private readonly StatusBarTextItem _statusBarItem = new() { Id = "LogView.Count", OrderIndex = -1 };

    private RadzenDataGrid<LogItemModel> _dataGrid = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    [Inject]
    private IUserSettingsService<LogView> UserSettings { get; set; } = default!;

    public override Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        Debug.Assert(Model.Session is not null, "Log view is opening without session");
        if (Model.Session is not null)
        {
            Model.Session.PropertiesChanged += OnPropertiesChanged;
            Model.Session.Refreshing += OnRefreshing;
        }

        return base.OnOpeningAsync(cancellationToken);
    }

    public override Task<IEnumerable<Toolbar>> OnCreateToolbarsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<Toolbar>)
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
                    }
                ]
            }
        ]);
    }

    public override Task<IEnumerable<StatusBarItem>> OnCreateStatusBarItemsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<StatusBarItem>)
        [
            _statusBarItem
        ]);
    }

    public override Task OnClosingAsync(CancellationToken cancellationToken)
    {
        if (Model.Session is not null)
        {
            Model.Session.PropertiesChanged -= OnPropertiesChanged;
            Model.Session.Refreshing -= OnRefreshing;
        }

        return base.OnClosingAsync(cancellationToken);
    }

    private IList<LogItemModel>? SelectedItems
    {
        get => Model.Session?.SelectedLogItem is null ? null : [Model.Session.SelectedLogItem];
        set
        {
            if (Model.Session is null) return;
            Model.Session.SelectedLogItem = value?.FirstOrDefault();
        }
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        //_dataGrid.RefreshDataAsync();
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
        if (Model.Session is null) return;

        try
        {
            await Model.Session.LoadDataAsync(args);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Update Data Grid", ex.Message);
        }

        OnUpdateStatusBarItem();
    }

    private (string icon, string color) GetLogEventIcon(LogEvent evt)
    {
        var logLevelSettings = SettingsService.GetSettings().GetLogLevelSettings(evt.Level);
        return (logLevelSettings.Icon, logLevelSettings.Color);
    }

    private Task OnEditSession(object? _)
    {
        return ViewManager.ShowViewAsync(ViewId.SessionConfig, CancellationToken.None);
    }

    private Task OnRefreshExecute(object? _)
    {
        Model.Session?.Refresh();
        return Task.CompletedTask;
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