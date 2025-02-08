using CodeYesterday.Lovi.Services;

namespace CodeYesterday.Lovi.Models;

public class ViewModel : IEquatable<ViewModel>
{
    public string Uri => Type switch
    {
        ViewType.StartView => "/",
        ViewType.SessionConfig => $"/session_config/{SessionId}",
        ViewType.LogView => $"/log/{SessionId}",
        ViewType.Settings => "/settings",
        _ => throw new ArgumentException("view type for Uri not implemented")
    };

    public ViewType Type { get; set; }

    public int SessionId { get; set; }

    public ulong SortOrder
    {
        get
        {
            var viewId = (short)Type;
            var sessionId = viewId < 0 ? 0xffffffffu : (uint)SessionId;
            viewId = Math.Abs(viewId);
            return sessionId << 16 | (ushort)viewId;
        }
    }

    public bool CanClose => Type switch
    {
        ViewType.SessionConfig or ViewType.LogView => true,
        _ => false
    };

    public bool IsSessionView => Type switch
    {
        ViewType.SessionConfig or ViewType.LogView => true,
        _ => false
    };

    public string Icon => Type switch
    {
        ViewType.StartView => "home",
        ViewType.SessionConfig => "edit",
        ViewType.LogView => "format_list_bulleted",
        ViewType.Settings => "settings",
        _ => string.Empty
    };

    public string? Title => CustomTitle ?? Type switch
    {
        ViewType.StartView => null,
        ViewType.SessionConfig => "Config view",
        ViewType.LogView => "Log view",
        ViewType.Settings => "settings",
        _ => string.Empty
    };

    public string? CustomTitle { get; set; }

    public bool Equals(ViewModel? other)
    {
        return string.Equals(Uri, other?.Uri, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ViewModel)obj);
    }

    public override int GetHashCode()
    {
        return Uri.GetHashCode();
    }
}
