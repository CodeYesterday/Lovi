using CodeYesterday.Lovi.Extensions;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components;

public partial class MultipleChoiceDialog
{
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public IList<MultipleChoiceOptionModel> Choices { get; set; } = [];

    [Parameter]
    public MultipleChoiceDialogOptions? DialogOptions { get; set; }

    private void OnCloseDialog(int? choiceId)
    {
        DialogService.Close(choiceId);
    }
}