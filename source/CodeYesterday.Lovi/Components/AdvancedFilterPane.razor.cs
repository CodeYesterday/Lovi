using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace CodeYesterday.Lovi.Components;

public partial class AdvancedFilterPane
{
    private RadzenDataFilter<LogItemModel>? _dataFilter;

    [Parameter] public string? CssClass { get; set; }

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