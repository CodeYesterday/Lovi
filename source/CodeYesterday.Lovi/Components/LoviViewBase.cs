using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components;

/// <summary>
/// Base clas for views and panes.
/// </summary>
[PublicAPI]
public abstract class LoviViewBase : ComponentBase
{
    [CascadingParameter]
    public ViewModel ViewModel { get; set; } = default!;

    /// <summary>
    /// Gets the internal view manager.
    /// </summary>
    [Inject]
    protected IViewManagerInternal ViewManagerInternal { get; set; } = default!;

    /// <summary>
    /// Gets the <see cref="Services.SessionService"/>.
    /// </summary>
    [Inject]
    protected ISessionService SessionService { get; set; } = default!;

    /// <summary>
    /// Gets the <see cref="ISettingsService"/>.
    /// </summary>
    [Inject]
    protected ISettingsService SettingsService { get; set; } = default!;

    /// <summary>
    /// Get the <see cref="IViewManager"/>.
    /// </summary>
    protected IViewManager ViewManager => ViewManagerInternal;

    /// <summary>
    /// The pane/view is about to be opened.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public virtual Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// The pane/view was opened.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public virtual Task OnOpenedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// The pane/view is about to be closed.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public virtual Task OnClosingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// The pane/view was closed.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public virtual Task OnClosedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}