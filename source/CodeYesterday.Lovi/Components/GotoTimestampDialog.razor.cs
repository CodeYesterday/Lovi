using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class GotoTimestampDialog
{
    public static async Task<DateTimeOffset?> ShowDialog(DialogService dialogService, DateTimeOffset? initialTimestamp)
    {
        return (DateTimeOffset?)await dialogService.OpenAsync<GotoTimestampDialog>("Goto timestamp",
            new Dictionary<string, object?> { { "InitialTimestamp", initialTimestamp } }, new()
            {
                CloseDialogOnEsc = true,
                Width = "auto"
            });
    }

    [Parameter]
    public DateTimeOffset? InitialTimestamp { get; set; }

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private DateTimeOffset Timestamp { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Timestamp = InitialTimestamp ?? DateTimeOffset.UtcNow;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }

    private void OnOk()
    {
        DialogService.Close(Timestamp);
    }

    private void OnCancel()
    {
        DialogService.Close();
    }
}