using CodeYesterday.Lovi.Session;

namespace CodeYesterday.Lovi.Models;

internal class AppModel
{
    private LogSession? _session;

    public LogSession? Session
    {
        get => _session;
        set
        {
            if (ReferenceEquals(value, _session)) return;
            var oldValue = _session;
            _session = value;
            SessionChanged?.Invoke(this, new(oldValue, value));
        }
    }

    public event EventHandler<ChangedEventArgs<LogSession>>? SessionChanged;
}