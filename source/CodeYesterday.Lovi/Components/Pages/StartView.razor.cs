using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using CodeYesterday.Lovi.Session;
using CommunityToolkit.Maui.Storage;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class StartView
{
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private IProgressIndicator ProgressIndicator { get; set; } = default!;

    [Inject]
    private IImporterManager ImporterManager { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IMruService MruService { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    private bool DisableButtons { get; set; }

    private async Task OnNewSession()
    {
        try
        {
            DisableButtons = true;

            var result = await FolderPicker.Default.PickAsync(CancellationToken.None).ConfigureAwait(true);

            if (result.Exception is not null) throw result.Exception;

            if (!result.IsSuccessful || result.Folder?.Path is null) return;

            var session = new LogSession(result.Folder.Path)
            {
                ImporterManager = ImporterManager
            };

            await session.CreateNewSessionAsync(LogSession.InMemorySessionDataStorageId, CancellationToken.None)
                .ConfigureAwait(true);

            MruService.SetMruSessionDirectory(session.SessionDirectory);

            Model.Session = session;

            NavigationManager.NavigateTo("/session_config");
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Create session failed", ex.Message, 20000d);
        }
        finally
        {
            DisableButtons = false;
            StateHasChanged();
        }
    }

    private async Task OnOpenSession(bool openConfigView)
    {
        try
        {
            DisableButtons = true;

            var result = await FolderPicker.Default.PickAsync(CancellationToken.None).ConfigureAwait(true);

            if (result.Exception is not null) throw result.Exception;

            if (!result.IsSuccessful || result.Folder?.Path is null) return;

            await OnOpenSession(result.Folder.Path, openConfigView).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                NotificationService.Notify(NotificationSeverity.Info, "Open session", "Cancelled");
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Open session", ex.Message, 20000d);
            }
        }
        finally
        {
            DisableButtons = false;
            StateHasChanged();
        }
    }

    private async Task OnOpenSession(string sessionDirectory, bool openConfigView)
    {
        try
        {
            DisableButtons = true;

            var session = new LogSession(sessionDirectory)
            {
                ImporterManager = ImporterManager
            };

            await session.OpenSessionAsync(CancellationToken.None).ConfigureAwait(true);

            MruService.SetMruSessionDirectory(session.SessionDirectory);

            string uri;

            if (openConfigView)
            {
                Model.Session = session;

                uri = "/session_config";
            }
            else
            {
                await session.ImportDataAsync(ProgressIndicator, CancellationToken.None).ConfigureAwait(true);

                uri = "/log";
            }

            Model.Session = session;

            NavigationManager.NavigateTo(uri);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                NotificationService.Notify(NotificationSeverity.Info, "Import data", "Cancelled");
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Import data", ex.Message, 20000d);
            }
        }
        finally
        {
            DisableButtons = false;
            StateHasChanged();
        }
    }
}