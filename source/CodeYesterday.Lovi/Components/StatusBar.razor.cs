using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components;

public partial class StatusBar
{
    private readonly List<StatusBarItem> _hookedItems = new();

    [CascadingParameter]
    public StatusBarModel StatusBarModel { get; set; } = default!;

    [Inject]
    private IProgressIndicator ProgressIndicator { get; set; } = default!;

    private ProgressModel? ProgressModel => ProgressIndicator.ProgressModel;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ProgressIndicator.ProgressModelChanged += OnProgressModelChanged;

        var progressModel = ProgressIndicator.ProgressModel;
        if (progressModel is not null)
        {
            progressModel.StateHasChanged += OnStateHasChanged;
        }

        StatusBarModel.ItemsChanged += StatusBarModelOnItemsChanged;
    }

    private void OnProgressModelChanged(object? sender, ChangedEventArgs<ProgressModel?> e)
    {
        if (e.OldValue is not null)
        {
            e.OldValue.StateHasChanged -= OnStateHasChanged;
        }

        if (e.NewValue is not null)
        {
            e.NewValue.StateHasChanged += OnStateHasChanged;
        }

        InvokeAsync(StateHasChanged);
    }

    private void OnStateHasChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void StatusBarModelOnItemsChanged(object? sender, EventArgs e)
    {
        foreach (var item in _hookedItems)
        {
            item.StateHasChanged -= OnStateHasChanged;
        }
        _hookedItems.Clear();

        foreach (var item in StatusBarModel.Items)
        {
            item.StateHasChanged += OnStateHasChanged;
            _hookedItems.Add(item);
        }

        InvokeAsync(StateHasChanged);
    }
}