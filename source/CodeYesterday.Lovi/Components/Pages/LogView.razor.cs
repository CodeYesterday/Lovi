using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Serilog.Events;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class LogView
{
    private Toolbar? _toolbar;
    private readonly StatusBarTextItem _statusBarItem = new() { Id = "LogView.Count", OrderIndex = -1 };

    private RadzenDataGrid<LogItemModel> _dataGrid = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    [Inject]
    private IUserSettingsService<LogView> UserSettings { get; set; } = default!;

    [CascadingParameter]
    public IPaneLayout? PaneLayout { get; set; }

    [CascadingParameter]
    public ToolbarContainer? ToolbarContainer { get; set; }

    [CascadingParameter]
    public StatusBarModel? StatusBarModel { get; set; }

    private IList<LogItemModel>? SelectedItems
    {
        get => Model.Session?.SelectedLogItem is null ? null : [Model.Session.SelectedLogItem];
        set
        {
            if (Model.Session is null) return;
            Model.Session.SelectedLogItem = value?.FirstOrDefault();
        }
    }

    protected override void OnParametersSet()
    {
        if (Model.Session is null) return;

        Model.Session.PropertiesChanged += OnPropertiesChanged;
        Model.Session.Refreshing += OnRefreshing;
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
            //Debug.Print($"LoadData: orderBy={args.OrderBy}, skip={args.Skip}, top={args.Top}");
            await Model.Session.LoadDataAsync(args);
        }
        catch (Exception ex)
        {
            //Debug.Print($"LoadDataException: {ex}");
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
        NavigationManager.NavigateTo("/session_config");
        return Task.CompletedTask;
    }

    private Task OnRefreshExecute(object? _)
    {
        Model.Session?.Refresh();
        return Task.CompletedTask;
    }
}