namespace CodeYesterday.Lovi.Models;

public class ProgressModel
{
    private string? _action;
    private string? _item;
    private ProgressData _mainProgress;
    private ProgressData? _secondaryProgress;
    private bool _canCancel = true;
    private Task? _stateHasChangedTask;

    public struct ProgressData
    {
        public double Max { get; set; }

        public double Value { get; set; }

        public double ValueAsPercent => Math.Round(Value / Max * 100d, 1);

        public bool IsIndeterminate { get; set; }
    }

    public required string Id { get; init; }

    public required string Action
    {
        get => _action ?? string.Empty;
        set
        {
            if (string.Equals(value, _action)) return;
            _action = value;
            OnStateHasChanged();
        }
    }

    public string? Item
    {
        get => _item;
        set
        {
            if (string.Equals(value, _item)) return;
            _item = value;
            OnStateHasChanged();
        }
    }

    public required ProgressData MainProgress
    {
        get => _mainProgress;
        set
        {
            if (Equals(value, MainProgress)) return;
            _mainProgress = value;
            OnStateHasChanged();
        }
    }

    public ProgressData? SecondaryProgress
    {
        get => _secondaryProgress;
        set
        {
            if (Equals(value, _secondaryProgress)) return;
            _secondaryProgress = value;
            OnStateHasChanged();
        }
    }

    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            if (value == _canCancel) return;
            _canCancel = value;
            OnStateHasChanged();
        }
    }

    public Func<Task>? CancelCallback { get; init; }

    public event EventHandler? StateHasChanged;

    private void OnStateHasChanged()
    {
        // Debounce changed event.
        if (_stateHasChangedTask is not null) return;

        _stateHasChangedTask = Task.Run(async () =>
        {
            await Task.Delay(100);
            StateHasChanged?.Invoke(this, EventArgs.Empty);
            _stateHasChangedTask = null;
        });
    }
}
