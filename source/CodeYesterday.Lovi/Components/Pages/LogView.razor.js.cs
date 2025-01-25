using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class LogView : IAsyncDisposable
{
    private IJSObjectReference? _jsModule;
    private int _rowHeight;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch
            {
                // ignored
            }
        }
    }

    private async Task ScrollToDataGridRowAsync(int rowIndex, CancellationToken cancellationToken = default)
    {
        if (!await CheckDataGridRowHeightAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext)) return;

        await _jsModule.InvokeVoidAsync("scrollToRow", cancellationToken, _dataGrid.UniqueID, rowIndex, _rowHeight).ConfigureAwait(true);
    }

    private async Task ScrollDataGridRowIntoViewAsync(int rowIndex, CancellationToken cancellationToken = default)
    {
        if (!await CheckDataGridRowHeightAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext)) return;

        await _jsModule.InvokeVoidAsync("scrollRowIntoView", cancellationToken, _dataGrid.UniqueID, rowIndex, _rowHeight).ConfigureAwait(true);
    }

    private async Task<int> GetDataGridRowHeightAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleLoadedAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return await _jsModule.InvokeAsync<int>("getRowHeight", cancellationToken, _dataGrid.UniqueID).ConfigureAwait(true);
    }

    [MemberNotNull(nameof(_jsModule))]
    private async Task<bool> CheckDataGridRowHeightAsync(CancellationToken cancellationToken)
    {
        await EnsureModuleLoadedAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        if (_rowHeight <= 0)
        {
            try
            {
                _rowHeight = await GetDataGridRowHeightAsync(cancellationToken);
            }
            catch
            {
                _rowHeight = 0;
            }
        }
        return _rowHeight > 0;
    }

    [MemberNotNull(nameof(_jsModule))]
    private async Task EnsureModuleLoadedAsync(CancellationToken cancellationToken)
    {
        if (_jsModule is not null) return;
        string jsFilePath = $"./Components/Pages/{nameof(LogView)}.razor.js";
        _jsModule = null!; // Required to suppress CS8774 in async method
        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, jsFilePath);
    }
}
