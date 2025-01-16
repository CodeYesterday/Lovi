using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FileInfo = System.IO.FileInfo;

namespace CodeYesterday.Lovi.Components.Pages;

public partial class SessionConfigView
{
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;

    [Inject]
    private IProgressIndicator ProgressIndicator { get; set; } = default!;

    [Inject]
    private IImporterManager ImporterManager { get; set; } = default!;

    [Inject]
    private AppModel Model { get; set; } = default!;

    private LogSession? Session => Model.Session;

    private List<ImportSourceViewModel> ImportSources { get; } = new();

    private IList<string> LogLevelFilterProperties { get; set; } = [];

    private string LogLevelPropertyInput { get; set; } = string.Empty;

    public override Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        base.OnInitialized();

        if (Session is null) return base.OnOpeningAsync(cancellationToken);

        foreach (var source in Session.SessionConfig.Sources)
        {
            var sourceView = new ImportSourceViewModel(source);
            ImportSources.Add(sourceView);
            RefreshSource(sourceView, source.SelectedFiles);
        }

        LogLevelFilterProperties = Session.SessionConfig.GetLogLevelFilterPropertySettings();

        return base.OnOpeningAsync(cancellationToken);
    }

    private void RefreshSource(ImportSourceViewModel source, IList<string>? selectedFiles = null)
    {
        if (Session is null || string.IsNullOrEmpty(Session.SessionDirectory)) return;

        try
        {
            if (selectedFiles is null)
            {
                source.RefreshFiles(Session.SessionDirectory);
            }
            else
            {
                source.RefreshFiles(selectedFiles, Session.SessionDirectory);
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error,
                $"Refresh source {source.Name}",
                ex.Message, closeOnClick: true);
        }
    }

    private void RemoveSource(ImportSourceViewModel source)
    {
        ImportSources.Remove(source);
    }

    private async Task OnUnloadData()
    {
        if (Session is null) return;

        try
        {
            await Session.UnloadDataAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Import failed", ex.Message, 20000d);
        }
    }

    private async Task OnImportData()
    {
        if (Session is null) return;

        try
        {
            await ImportDataAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                NotificationService.Notify(NotificationSeverity.Info, "Import data", "Cancelled");
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Import failed", ex.Message, 20000d);
            }
        }
    }

    private async Task OnOpenSession()
    {
        if (Session is null) return;

        try
        {
            await ImportDataAsync(CancellationToken.None).ConfigureAwait(true);

            await ViewManager.ShowViewAsync(ViewId.LogView, CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                NotificationService.Notify(NotificationSeverity.Info, "Import data", "Cancelled");
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Import failed", ex.Message, 20000d);
            }
        }
    }

    private async Task ImportDataAsync(CancellationToken cancellationToken)
    {
        if (Session is null) return;

        Session.SessionConfig.Sources.Clear();

        foreach (var source in ImportSources)
        {
            Session.SessionConfig.Sources.Add(source.GetImportSource());
        }

        Session.SessionConfig.WriteLogLevelFilterPropertySettings(LogLevelFilterProperties);

        await Session.SaveDataAsync(cancellationToken).ConfigureAwait(true);

        await Session.ImportDataAsync(ProgressIndicator, cancellationToken).ConfigureAwait(true);
    }

    private void AddSource()
    {
        if (Session is null) return;

        var importerProfile = ImporterManager.ImporterProfiles.Values.FirstOrDefault();
        if (importerProfile is null) return;

        var source = new ImportSourceViewModel
        {
            Name = importerProfile.Name,
            Filter = importerProfile.DefaultSourceFilter,
            ImporterProfileId = importerProfile.Id,
            ImportAllFiles = true
        };
        ImportSources.Add(source);

        RefreshSource(source);
    }

    private void OnRemoveLogLevelFilterProperty(string property)
    {
        LogLevelFilterProperties.Remove(property);
    }

    private void OnAddLogLevelFilterProperty()
    {
        var property = LogLevelPropertyInput.Trim();

        if (string.IsNullOrEmpty(property)) return;
        if (LogLevelFilterProperties.Contains(property)) return;

        LogLevelFilterProperties.Add(property);
    }
}

public class ImportSourceViewModel
{
    public required string Name { get; init; }

    public required string ImporterProfileId { get; init; }

    public required string Filter { get; set; }

    public bool ImportAllFiles { get; set; }

    public List<ImportSourceFileViewModel> Files { get; } = new();

    public IEnumerable<ImportSourceFileViewModel> SortedFiles => Files.OrderBy(f => f.FileName);

    public ImportSourceViewModel()
    { }

    [SetsRequiredMembers]
    public ImportSourceViewModel(ImportSource importSource)
    {
        Name = importSource.Name;
        ImporterProfileId = importSource.ImporterProfileId;
        Filter = importSource.Filter;
        ImportAllFiles = importSource.ImportAllFiles;
    }

    public void RefreshFiles(string sessionDirectory)
    {
        RefreshFiles(Files.Where(f => f.IsSelected).Select(f => f.FileName).ToArray(), sessionDirectory);
    }

    public void RefreshFiles(IList<string> selectedFiles, string sessionDirectory)
    {
        var dir = Path.GetDirectoryName(Filter);
        if (string.IsNullOrEmpty(dir))
        {
            dir = sessionDirectory;
        }
        else if (!Path.IsPathRooted(dir))
        {
            dir = Path.Combine(sessionDirectory, dir);
        }
        Files.Clear();
        Files.AddRange(Directory.GetFileSystemEntries(dir, Path.GetFileName(Filter))
            .Select(f => new FileInfo(f))
            .Select(fi => new ImportSourceFileViewModel
            {
                FileName = fi.FullName,
                FileSize = fi.Length,
                FileTime = fi.LastWriteTime
            }));

        foreach (var selectedFile in selectedFiles)
        {
            var sourceFile =
                Files.FirstOrDefault(sf => string.Equals(sf.FileName, Path.GetFullPath(selectedFile), StringComparison.Ordinal));
            if (sourceFile is null)
            {
                Files.Add(new()
                {
                    FileName = selectedFile,
                    FileSize = 0,
                    FileTime = DateTime.MinValue,
                    IsSelected = true,
                    IsMissing = true
                });
            }
            else
            {
                sourceFile.IsSelected = true;
            }
        }
    }

    public ImportSource GetImportSource()
    {
        var source = new ImportSource
        {
            Name = Name,
            ImporterProfileId = ImporterProfileId,
            Filter = Filter,
            ImportAllFiles = ImportAllFiles
        };

        if (!ImportAllFiles)
        {
            source.SelectedFiles = Files
                .Where(f => f is { IsSelected: true, IsMissing: false })
                .Select(f => f.FileName).ToArray();
        }

        return source;
    }
}

public class ImportSourceFileViewModel
{
    public required string FileName { get; init; }

    public required long FileSize { get; init; }

    public required DateTime FileTime { get; init; }

    public bool IsSelected { get; set; }

    public bool IsMissing { get; set; }

    public string GetRelativeFileName(string sessionDirectory, string filter)
    {
        var dir = Path.GetDirectoryName(filter);
        if (string.IsNullOrEmpty(dir))
        {
            dir = sessionDirectory;
        }
        else if (!Path.IsPathRooted(dir))
        {
            dir = Path.Combine(sessionDirectory, dir);
        }

        return Path.GetRelativePath(dir, FileName);
    }

    public string GetDisplayFileSize()
    {
        if (IsMissing) return string.Empty;

        const long kb = 1024;
        const long mb = kb * kb;

        return FileSize switch
        {
            >= mb => $"{Math.Round(FileSize / (double)mb, 3)} MB",
            >= kb => $"{Math.Round(FileSize / (double)kb, 3)} kB",
            _ => $"{FileSize} Byte"
        };
    }

    public string GetDisplayFileTime()
    {
        if (IsMissing) return string.Empty;

        return FileTime.ToString("G", CultureInfo.CurrentUICulture);
    }
}