﻿using CodeYesterday.Lovi.Extensions;
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
    private DialogService DialogService { get; set; } = default!;

    [Inject]
    private IProgressIndicator ProgressIndicator { get; set; } = default!;

    [Inject]
    private IImporterManager ImporterManager { get; set; } = default!;

    [Inject]
    private IMruService MruService { get; set; } = default!;

    private bool DisableButtons { get; set; }

    private async Task OnNewSession()
    {
        try
        {
            DisableButtons = true;

            var result = await FolderPicker.Default.PickAsync(CancellationToken.None).ConfigureAwait(true);

            if (result.Exception is not null) throw result.Exception;

            var sessionDirectory = result.Folder?.Path;
            if (!result.IsSuccessful || sessionDirectory is null) return;

            if (LogSession.SessionInfoFileExists(sessionDirectory))
            {
                var choice = await DialogService.ShowMultipleChoiceDialogAsync(
                    "Log session found in directory",
                    "The selected directory already contains a log session.",
                    [
                        new MultipleChoiceOptionModel
                        {
                            Id = 1,
                            Title = "Edit session",
                            Icon = "folder_open",
                            Description = "Open the log session in edit mode instead.",
                            ButtonText = "Edit session",
                            ButtonStyle = ButtonStyle.Primary
                        },
                        new MultipleChoiceOptionModel
                        {
                            Id = 2,
                            Title = "Open session",
                            Icon = "edit",
                            Description = "Open the log session instead.",
                            ButtonText = "Open session",
                            ButtonStyle = ButtonStyle.Secondary
                        },
                        new MultipleChoiceOptionModel
                        {
                            Id = 3,
                            Title = "Overwrite session",
                            Icon = "add_circle",
                            IconStyle = IconStyle.Danger,
                            Description = "Open the log session instead.",
                            ButtonText = "Create new session",
                            ButtonStyle = ButtonStyle.Danger
                        },
                        new MultipleChoiceOptionModel
                        {
                            Id = null,
                            Title = "Cancel",
                            Icon = "cancel",
                            Description = "Do nothing.",
                            ButtonText = "Cancel",
                            ButtonStyle = ButtonStyle.Base
                        }
                    ]).ConfigureAwait(true);

                switch (choice)
                {
                    case 1:
                    case 2:
                        await OnOpenSession(sessionDirectory, choice == 1).ConfigureAwait(true);
                        return;

                    case 3:
                        // continue below
                        break;

                    default: return;
                }
            }

            await OnNewSessionAsync(sessionDirectory).ConfigureAwait(true);
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

    private async Task OnNewSessionAsync(string sessionDirectory)
    {
        var session = new LogSession(sessionDirectory)
        {
            ImporterManager = ImporterManager
        };

        await session.CreateNewSessionAsync(LogSession.InMemorySessionDataStorageId, CancellationToken.None)
            .ConfigureAwait(true);

        MruService.SetMruSessionDirectory(session.SessionDirectory);

        Model.Session = session;

        await ViewManager.ShowViewAsync(ViewId.SessionConfig, CancellationToken.None).ConfigureAwait(true);
    }

    private async Task OnOpenSession(bool openConfigView)
    {
        try
        {
            DisableButtons = true;

            var result = await FolderPicker.Default.PickAsync(CancellationToken.None).ConfigureAwait(true);

            if (result.Exception is not null) throw result.Exception;

            var sessionDirectory = result.Folder?.Path;
            if (!result.IsSuccessful || sessionDirectory is null) return;

            if (!LogSession.SessionInfoFileExists(sessionDirectory))
            {
                var choice = await DialogService.ShowMultipleChoiceDialogAsync(
                    "Log session not found in directory",
                    "The selected directory does not contains a log session.",
                    [
                        new MultipleChoiceOptionModel
                        {
                            Id = 1,
                            Title = "Create new session",
                            Icon = "add_circle",
                            Description = "Create a new session instead.",
                            ButtonText = "Create session",
                            ButtonStyle = ButtonStyle.Primary
                        },
                        new MultipleChoiceOptionModel
                        {
                            Id = null,
                            Title = "Cancel",
                            Icon = "cancel",
                            Description = "Do nothing.",
                            ButtonText = "Cancel",
                            ButtonStyle = ButtonStyle.Base
                        }
                    ])
                    .ConfigureAwait(true);

                if (choice == 1)
                {
                    await OnNewSessionAsync(sessionDirectory).ConfigureAwait(true);
                }
                return;
            }

            await OnOpenSession(sessionDirectory, openConfigView).ConfigureAwait(true);
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

            ViewId viewId;

            if (openConfigView)
            {
                Model.Session = session;

                viewId = ViewId.SessionConfig;
            }
            else
            {
                await session.ImportDataAsync(ProgressIndicator, CancellationToken.None).ConfigureAwait(true);

                viewId = ViewId.LogView;
            }

            Model.Session = session;

            await ViewManager.ShowViewAsync(viewId, CancellationToken.None).ConfigureAwait(true);
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