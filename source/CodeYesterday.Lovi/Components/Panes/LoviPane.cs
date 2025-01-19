namespace CodeYesterday.Lovi.Components;

/// <summary>
/// Base class for panes.
/// </summary>
/// <remarks>
/// A pane is a Blazor component, but not a page.
/// </remarks>
[PublicAPI]
public abstract class LoviPane : LoviViewBase
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(true);

        await ViewManagerInternal.OnOpeningPaneAsync(this).ConfigureAwait(false);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);

        if (firstRender)
        {
            await ViewManagerInternal.OnPaneOpenedAsync(this).ConfigureAwait(true);
        }
    }
}