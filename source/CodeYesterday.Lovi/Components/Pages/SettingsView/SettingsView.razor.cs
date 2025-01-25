using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components.Pages.SettingsView;

public partial class SettingsView
{
    public static readonly List<SettingCategoryModel> Categories =
    [
        new()
        {
            Title = "Visuals",
            Icon = "monitor",
            Pages =
            [
                new()
                {
                    Title = "Log levels",
                    PageType = typeof(LogLevelVisualsSettingsPage)
                }
            ]
        }
    ];

    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    private SettingsModel? Settings { get; set; }

    private SettingPageModel SelectedPage { get; set; } = Categories.First().Pages.First();

    public override async Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        // TODO: disable home and settings button in toolbar.

        await base.OnOpeningAsync(cancellationToken).ConfigureAwait(true);

        Settings = SettingsService.Settings;
    }

    public override Task<(IEnumerable<Toolbar> toolbars, IEnumerable<KeyboardShortcut> shortcuts)> OnCreateToolbarsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((
            (IEnumerable<Toolbar>)
            [
                new Toolbar
                {
                    Id = "Settings",
                    OrderIndex = -2,
                    AddGap = true,
                    Items =
                    [
                        new ToolbarButton
                        {
                            Title = "Save",
                            Icon = "check_circle",
                            ButtonVariant = Variant.Flat,
                            ButtonStyle = ButtonStyle.Success,
                            Tooltip = "Save and close settings",
                            Command = new AsyncCommand(OnSaveAndCloseAsync)
                        },
                        new ToolbarButton
                        {
                            Title = "Cancel",
                            Icon = "cancel",
                            ButtonVariant = Variant.Flat,
                            ButtonStyle = ButtonStyle.Base,
                            Tooltip = "Discard and close settings",
                            Command = new AsyncCommand(OnDiscardAndCloseAsync)
                        }
                    ]
                }
            ],
            (IEnumerable<KeyboardShortcut>)[]));
    }

    private void ShowPage(SettingPageModel page)
    {
        SelectedPage = page;

        StateHasChanged();
    }

    private async Task OnSaveAndCloseAsync(object? parameter)
    {
        try
        {
            await SettingsService.SaveSettingsAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Save settings failed", ex.Message, 20000d);
        }

        await ViewManager.NavigateBackAsync(CancellationToken.None).ConfigureAwait(true);
    }

    private async Task OnDiscardAndCloseAsync(object? parameter)
    {
        try
        {
            await SettingsService.LoadSettingsAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Load settings failed", ex.Message, 20000d);
        }

        await ViewManager.NavigateBackAsync(CancellationToken.None).ConfigureAwait(true);
    }

    private RenderFragment<SettingPageModel> RenderPage => page => builder =>
    {
        builder.OpenComponent(1, page.PageType);
        builder.CloseComponent();
    };
}

public record SettingCategoryModel
{
    public required string Title { get; init; }

    public required string Icon { get; init; }

    public required List<SettingPageModel> Pages { get; init; }
}

public record SettingPageModel
{
    public required string Title { get; init; }

    public string? Icon { get; init; }

    public required Type PageType { get; init; }
}
