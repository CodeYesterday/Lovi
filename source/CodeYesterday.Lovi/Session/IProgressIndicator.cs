using CodeYesterday.Lovi.Models;

namespace CodeYesterday.Lovi.Session;

public interface IProgressIndicator
{
    ProgressModel? ProgressModel { get; }

    event EventHandler<ChangedEventArgs<ProgressModel?>>? ProgressModelChanged;

    void SetProgressModel(ProgressModel model);

    void ClearProgressModel(string id);
}
