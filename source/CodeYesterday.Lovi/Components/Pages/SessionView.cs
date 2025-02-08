using CodeYesterday.Lovi.Session;
using Microsoft.AspNetCore.Components;

namespace CodeYesterday.Lovi.Components.Pages;

public abstract class SessionView : LoviView
{
    [Parameter]
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
}