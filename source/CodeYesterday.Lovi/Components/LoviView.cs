using CodeYesterday.Lovi.Components.Layout;
using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components;

/// <summary>
/// Base class for views.
/// </summary>
/// <remarks>
/// A view is a Blazor page.
/// </remarks>
[PublicAPI]
public abstract class LoviView : LoviViewBase
{
    /// <summary>
    /// Gets or sets the <see cref="ToolbarContainer"/>.
    /// </summary>
    /// <remarks>
    /// Is set by the <see cref="MainLayout"/> as a cascading parameter.
    /// </remarks>
    [CascadingParameter]
    public ToolbarContainer? ToolbarContainer { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="StatusBarModel"/>.
    /// </summary>
    /// <remarks>
    /// Is set by the <see cref="MainLayout"/> as a cascading parameter.
    /// </remarks>
    [CascadingParameter]
    public StatusBarModel? StatusBarModel { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPaneLayout"/>.
    /// </summary>
    /// <remarks>
    /// Is set by the <see cref="Components.Layout.PaneLayout"/> as a cascading parameter.
    /// </remarks>
    [CascadingParameter]
    public IPaneLayout? PaneLayout { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync().ConfigureAwait(true);

        await ViewManagerInternal.OnOpeningViewAsync(this).ConfigureAwait(true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);

        if (firstRender)
        {
            await ViewManagerInternal.OnViewOpenedAsync(this).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Implement to create the panes in a <see cref="Components.Layout.PaneLayout"/> view.
    /// </summary>
    /// <param name="paneLayout">The <see cref="IPaneLayout"/> to add the panes to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public virtual Task OnCreatePanesAsync(IPaneLayout paneLayout, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Implement to create the toolbars.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns an enumeration with the created toolbars.</returns>
    public virtual Task<IEnumerable<Toolbar>> OnCreateToolbarsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<Toolbar>)[]);
    }

    /// <summary>
    /// Implement to create the status bar items.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Returns an enumeration with the created status bar items.</returns>
    public virtual Task<IEnumerable<StatusBarItem>> OnCreateStatusBarItemsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IEnumerable<StatusBarItem>)[]);
    }
}
