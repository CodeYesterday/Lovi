using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeYesterday.Lovi.Components;

public partial class LogEventMessagePane
{
    private static readonly Regex StackTraceRegex = new(@"^\s*at (?<method>.+?) in (?<file>.+?):line (?<line>\d+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Parameter]
    public string? CssClass { get; set; }

    [Inject]
    private ISettingsService SettingsService { get; set; } = default!;

    [Inject]
    private AppModel AppModel { get; set; } = default!;

    private LogItemModel? SelectedItem => AppModel.Session?.SelectedLogItem;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        AppModel.SessionChanged += OnSessionChanged;

        if (AppModel.Session is not null)
        {
            AppModel.Session.SelectedLogItemModelChanged += OnSelectedLogItemModelChanged;
        }
    }

    private void OnSessionChanged(object? sender, ChangedEventArgs<LogSession> e)
    {
        if (e.OldValue is not null)
        {
            e.OldValue.SelectedLogItemModelChanged -= OnSelectedLogItemModelChanged;
        }

        if (e.NewValue is not null)
        {
            e.NewValue.SelectedLogItemModelChanged += OnSelectedLogItemModelChanged;
        }
    }

    private void OnSelectedLogItemModelChanged(object? sender, ChangedEventArgs<LogItemModel> e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task CopyMessage()
    {
        if (SelectedItem != null)
        {
            await Clipboard.SetTextAsync(SelectedItem.LogEvent.RenderMessage());
        }
    }

    private async Task CopyException()
    {
        if (SelectedItem?.LogEvent.Exception != null)
        {
            await Clipboard.SetTextAsync(SelectedItem.LogEvent.Exception.ToString());
        }
    }

    private static RenderFragment<LogItemModel> RenderedException => logItem => builder =>
    {
        if (logItem.LogEvent.Exception is null) return;

        var lines = logItem.LogEvent.Exception.ToString().Split('\n');

        var sb = new StringBuilder();
        bool firstLine = true;
        foreach (var line in lines)
        {
            if (firstLine)
            {
                firstLine = false;
            }
            else
            {
                sb.Append("<br/>");
            }

            var match = StackTraceRegex.Match(line);
            if (match.Success)
            {
                ReplaceGroups(sb, line, match, (_, name, original) =>
                {
                    switch (name)
                    {
                        case "method": return $"<span class=\"slm-property-name\">{original}</span>";
                        case "file": return $"<span class=\"slm-string-value\">{original}</span>";
                        case "line": return $"<span class=\"slm-numeric-value\">{original}</span>";

                        default: return original;
                    }
                });
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        builder.AddMarkupContent(0, sb.ToString());
    };

    private static void ReplaceGroups(StringBuilder sb, string text, Match match, Func<int, string, string, string> groupReplaceCallback)
    {
        int lastEnd = 0;
        for (int index = 0; index < match.Groups.Count; ++index)
        {
            var group = match.Groups[index]!;
            if (group is Match) continue;

            if (group.Index > lastEnd)
            {
                sb.Append(text.Substring(lastEnd, group.Index - lastEnd));
            }

            sb.Append(groupReplaceCallback(index, group.Name, group.Value));

            lastEnd = group.Index + group.Length;
        }

        if (text.Length > lastEnd)
        {
            sb.Append(text.Substring(lastEnd));
        }
    }
}