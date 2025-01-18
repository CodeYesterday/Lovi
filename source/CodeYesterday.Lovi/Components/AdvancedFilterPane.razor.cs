using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace CodeYesterday.Lovi.Components;

public partial class AdvancedFilterPane
{
    private RadzenDataFilter<LogItemModel>? _dataFilter;

    [Parameter] public string? CssClass { get; set; }

    [Inject]
    private AppModel Model { get; set; } = default!;

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    private Task ApplyFilter()
    {
        if (_dataFilter is null) return Task.CompletedTask;

        return _dataFilter.Filter();
    }

    private void ViewChanged()
    {
        if (Model.Session is null) return;

        Model.Session.AdvancedFilterExpression = _dataFilter.ToFilterString();
    }
}