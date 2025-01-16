using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components;

[PublicAPI]
public abstract class LoviView : ComponentBase
{
    [Inject]
    private IViewManagerInternal ViewManagerInternal { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await ViewManagerInternal.OnOpeningViewAsync(this).ConfigureAwait(true);

        await base.OnInitializedAsync().ConfigureAwait(true);
    }

    [CascadingParameter]
    public ToolbarContainer? ToolbarContainer { get; set; }

    [CascadingParameter]
    public StatusBarModel? StatusBarModel { get; set; }

    protected IViewManager ViewManager => ViewManagerInternal;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ViewManagerInternal.OnViewOpenedAsync(this).ConfigureAwait(true);
        }

        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
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
}
