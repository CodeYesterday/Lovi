using CodeYesterday.Lovi.Models;

namespace CodeYesterday.Lovi.Session;

internal class ProgressIndicator : IProgressIndicator
{
    private ProgressModel? _progressModel;

    public ProgressModel? ProgressModel
    {
        get => _progressModel;
        private set
        {
            if (ReferenceEquals(value, _progressModel)) return;
            var oldModel = _progressModel;
            _progressModel = value;
            ProgressModelChanged?.Invoke(this, new(oldModel, value));
        }
    }

    public event EventHandler<ChangedEventArgs<ProgressModel?>>? ProgressModelChanged;

    public void SetProgressModel(ProgressModel model)
    {
        if (ProgressModel is not null && !string.Equals(ProgressModel.Id, model.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"The progress indicator is already in use by {ProgressModel?.Id}");
        }

        ProgressModel = model;
    }

    public void ClearProgressModel(string id)
    {
        if (ProgressModel is not null && string.Equals(ProgressModel.Id, id, StringComparison.Ordinal))
        {
            ProgressModel = null;
        }
    }
}
