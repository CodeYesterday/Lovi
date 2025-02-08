using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components.Panes;

public abstract class SessionPane : LoviPane
{
    [CascadingParameter(Name = nameof(SessionId))]
    public int SessionId { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="LogSession"/>. If <see langword="null"/> then no session is loaded.
    /// </summary>
    protected LogSession? Session { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue(nameof(SessionId), out int sessionId) &&
            (Session is null || Session.SessionId != sessionId))
        {
            Session = SessionService.GetSession(sessionId);
        }

        return base.SetParametersAsync(parameters);
    }

    protected override void OnInitialized()
    {
        if (SessionId > 0 &&
            (Session is null || Session.SessionId != SessionId))
        {
            Session = SessionService.GetSession(SessionId);
        }

        base.OnInitialized();
    }
}