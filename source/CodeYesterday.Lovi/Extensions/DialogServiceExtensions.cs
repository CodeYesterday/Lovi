using CodeYesterday.Lovi.Components;
using Radzen;

namespace CodeYesterday.Lovi.Extensions;

public static class DialogServiceExtensions
{
    public static async Task<int?> ShowMultipleChoiceDialogAsync(this DialogService dialogService,
        string title, string? text,
        IEnumerable<MultipleChoiceOptionModel> choices,
        MultipleChoiceDialogOptions? dialogOptions = null)
    {
        var result = await dialogService.OpenAsync<MultipleChoiceDialog>(
            title,
            new Dictionary<string, object?>
            {
                { "Text", text },
                { "Choices", choices.ToArray() },
                { "DialogOptions", dialogOptions }
            },
            dialogOptions).ConfigureAwait(true);

        return result as int?;
    }
}

public class MultipleChoiceDialogOptions : DialogOptions
{
    public ButtonStyle DefaultButtonStyle { get; set; } = ButtonStyle.Secondary;
}

public record MultipleChoiceOptionModel
{
    public required int? Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public string? Icon { get; init; }

    public string? IconColor { get; init; }

    public IconStyle? IconStyle { get; init; }

    public required string ButtonText { get; init; }

    public ButtonStyle? ButtonStyle { get; init; }

    public string? ButtonIcon { get; init; }

    public string? ButtonIconColor { get; init; }
}
