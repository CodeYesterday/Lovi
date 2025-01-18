using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components;

[PublicAPI]
public abstract class LoviView : ComponentBase
{
    [Inject]
    private IViewManagerInternal ViewManagerInternal { get; set; } = default!;

    [CascadingParameter]
    public ToolbarContainer? ToolbarContainer { get; set; }

    [CascadingParameter]
    public StatusBarModel? StatusBarModel { get; set; }

    [CascadingParameter]
    public IPaneLayout? PaneLayout { get; set; }

    protected IViewManager ViewManager => ViewManagerInternal;

    protected override async Task OnInitializedAsync()
    {
        await ViewManagerInternal.OnOpeningViewAsync(this).ConfigureAwait(true);

        await base.OnInitializedAsync().ConfigureAwait(true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ViewManagerInternal.OnViewOpenedAsync(this).ConfigureAwait(true);
        }

        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
    }

    public virtual Task OnCreatePanesAsync(IPaneLayout paneLayout, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnOpenedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnClosingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnClosedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task<IEnumerable<Toolbar>> OnCreateToolbarsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<Toolbar>)[]);
    }

    public virtual Task<IEnumerable<StatusBarItem>> OnCreateStatusBarItemsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<StatusBarItem>)[]);
    }
}
