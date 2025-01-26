using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeYesterday.Lovi.Components.Panes;

public partial class LogEventMessagePane
{
    private static readonly Regex StackTraceRegex = new(@"^\s*at (?<method>.+?) in (?<file>.+?):line (?<line>\d+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Parameter]
    public string? CssClass { get; set; }

    [Inject]
    private IUserSettingsService<LogEventMessagePane> UserSettings { get; set; } = default!;

    private LogItemModel? SelectedItem => Session?.SelectedLogItem;

    private ToolbarButton CopyMessageButton { get; }

    private ToolbarButton CopyExceptionButton { get; }

    private ToolbarCheckBox WrapMessageCheckBox { get; }

    private ToolbarCheckBox WrapExceptionCheckBox { get; }

    private double MessagePaneWidth { get; set; } = 50d;

    private bool ExceptionPaneVisible { get; set; } = true;

    public LogEventMessagePane()
    {
        CopyMessageButton = new()
        {
            Icon = "content_copy",
            Tooltip = "Copy message to clipboard",
            ButtonSize = ButtonSize.Small,
            Command = new AsyncCommand(OnCopyMessage, OnCopyMessageCanExecute)
        };
        CopyExceptionButton = new()
        {
            Icon = "content_copy",
            Tooltip = "Copy exception to clipboard",
            ButtonSize = ButtonSize.Small,
            Command = new AsyncCommand(OnCopyException, OnCopyExceptionCanExecute)
        };

        WrapMessageCheckBox = new()
        {
            Icon = "wrap_text",
            Tooltip = "Wrap message text",
            ButtonSize = ButtonSize.Small,
            IsChecked = true
        };
        WrapMessageCheckBox.IsCheckedChanged += WrapCheckBoxOnIsCheckedChanged;
        WrapExceptionCheckBox = new()
        {
            Icon = "wrap_text",
            Tooltip = "Wrap exception text",
            ButtonSize = ButtonSize.Small,
            IsChecked = true
        };
        WrapExceptionCheckBox.IsCheckedChanged += WrapCheckBoxOnIsCheckedChanged;
    }

    private void WrapCheckBoxOnIsCheckedChanged(object? sender, ChangedEventArgs<bool> e)
    {
        UserSettings.SetValue("WrapMessage", WrapMessageCheckBox.IsChecked);
        UserSettings.SetValue("WrapException", WrapExceptionCheckBox.IsChecked);

        InvokeAsync(StateHasChanged);
    }

    public override async Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        await base.OnOpeningAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);

        if (Session is not null)
        {
            Session.SelectedLogItemModelChanged += OnSelectedLogItemModelChanged;
        }
    }

    public override async Task OnOpenedAsync(CancellationToken cancellationToken)
    {
        WrapMessageCheckBox.IsChecked = UserSettings.GetValue("WrapMessage", true);
        WrapExceptionCheckBox.IsChecked = UserSettings.GetValue("WrapException", true);

        MessagePaneWidth = UserSettings.GetValue("MessagePaneWidth", 50d);
        ExceptionPaneVisible = UserSettings.GetValue("ExceptionPaneVisible", true);

        StateHasChanged();

        await base.OnOpenedAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
    }

    public override Task OnClosingAsync(CancellationToken cancellationToken)
    {
        if (Session is not null)
        {
            Session.SelectedLogItemModelChanged -= OnSelectedLogItemModelChanged;
        }

        return base.OnClosingAsync(cancellationToken);
    }

    private void OnSelectedLogItemModelChanged(object? sender, ChangedEventArgs<LogItemModel> e)
    {
        InvokeAsync(StateHasChanged);

        ((AsyncCommand)CopyMessageButton.Command).NotifyCanExecuteChanged();
        ((AsyncCommand)CopyExceptionButton.Command).NotifyCanExecuteChanged();
    }

    private async Task OnCopyMessage(object? parameter)
    {
        if (SelectedItem is not null)
        {
            await Clipboard.SetTextAsync(SelectedItem.LogEvent.RenderMessage()).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        }
    }

    private bool OnCopyMessageCanExecute(object? parameter)
    {
        return SelectedItem is not null;
    }

    private async Task OnCopyException(object? parameter)
    {
        if (SelectedItem?.LogEvent.Exception is not null)
        {
            await Clipboard.SetTextAsync(SelectedItem.LogEvent.Exception.ToString()).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
        }
    }

    private bool OnCopyExceptionCanExecute(object? parameter)
    {
        return SelectedItem?.LogEvent.Exception is not null;
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
            var group = match.Groups[index];
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

    private void OnSplitterPaneExpand(RadzenSplitterEventArgs args)
    {
        if (args.PaneIndex == 1)
        {
            UserSettings.SetValue("ExceptionPaneVisible", true);
        }
    }

    private void OnSplitterPaneCollapse(RadzenSplitterEventArgs args)
    {
        if (args.PaneIndex == 1)
        {
            UserSettings.SetValue("ExceptionPaneVisible", false);
        }
    }

    private void OnSplitterPaneResize(RadzenSplitterResizeEventArgs args)
    {
        if (args.PaneIndex == 0)
        {
            UserSettings.SetValue("MessagePaneWidth", args.NewSize);
        }
    }
}