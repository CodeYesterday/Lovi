using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using System.Text;

namespace CodeYesterday.Lovi.Components;

public partial class InfoButton
{
    private RadzenButton _button = default!;
    private Popup _popup = default!;

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? Height { get; set; }

    private string GetPopupStyle()
    {
        var sb = new StringBuilder();

        if (Width is not null)
        {
            sb.Append($"width: {Width};");
        }
        if (Height is not null)
        {
            sb.Append($"height: {Height};");
        }

        return sb.ToString();
    }
}