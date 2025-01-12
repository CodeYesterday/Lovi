using System.Diagnostics.CodeAnalysis;

namespace CodeYesterday.Lovi.Services;

/// <summary>
/// The MAUI application implementation of <see cref="IUserSettingsService"/> using <see cref="Preferences"/> to store the values.
/// </summary>
/// <typeparam name="TInstance">The type of the instantiating object. The <see cref="Type.Name"/> is used as a prefix for all value IDs.</typeparam>
/// <remarks>
/// Type mapping:
/// <ul>
/// <li>
/// <see cref="string" />, <see cref="bool" />, <see cref="int" />, <see cref="long" />, <see cref="float" />, <see cref="double" /> and <see cref="DateTime" />
/// are stored natively.
/// </li>
/// <li>
/// <see cref="byte"/>, <see cref="short"/> and <see cref="uint"/> are stored as <see cref="int"/>.
/// </li>
/// <li>
/// <see cref="ulong"/> is stored as <see cref="string"/>.
/// </li>
/// </ul>
/// </remarks>
internal class MauiUserSettingsService<TInstance> : IUserSettingsService<TInstance>
{
    private string InstancePrefix { get; } = typeof(TInstance).Name + ".";

    public void SetValue<T>(string id, T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        switch (value)
        {
            case string:
            case bool:
            case int:
            case long:
            case float:
            case double:
            case DateTime:
                Preferences.Default.Set(InstancePrefix + id, value);
                break;

            case byte by:
                Preferences.Default.Set(InstancePrefix + id, (int)by);
                break;

            case short sh:
                Preferences.Default.Set(InstancePrefix + id, (int)sh);
                break;

            case ushort ush:
                Preferences.Default.Set(InstancePrefix + id, (int)ush);
                break;

            case uint ui:
                Preferences.Default.Set(InstancePrefix + id, (long)ui);
                break;

            case ulong ul:
                Preferences.Default.Set(InstancePrefix + id, ul.ToString());
                break;

            default: throw new ArgumentException($"Type {value.GetType()} is not supported", nameof(value));
        }
    }

    public string GetValue(string id, string defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public bool GetValue(string id, bool defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public byte GetValue(string id, byte defaultValue)
    {
        return (byte)Preferences.Default.Get(InstancePrefix + id, (int)defaultValue);
    }

    public short GetValue(string id, short defaultValue)
    {
        return (short)Preferences.Default.Get(InstancePrefix + id, (int)defaultValue);
    }

    public ushort GetValue(string id, ushort defaultValue)
    {
        return (ushort)Preferences.Default.Get(InstancePrefix + id, (int)defaultValue);
    }

    public int GetValue(string id, int defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public uint GetValue(string id, uint defaultValue)
    {
        return (uint)Preferences.Default.Get(InstancePrefix + id, (long)defaultValue);
    }

    public long GetValue(string id, long defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public ulong GetValue(string id, ulong defaultValue)
    {
        if (ulong.TryParse(Preferences.Default.Get(InstancePrefix + id, defaultValue.ToString()), out var ul)) return ul;
        return defaultValue;
    }

    public double GetValue(string id, double defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public float GetValue(string id, float defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public DateTime GetValue(string id, DateTime defaultValue)
    {
        return Preferences.Default.Get(InstancePrefix + id, defaultValue);
    }

    public bool TryGetValue(string id, [NotNullWhen(true)] out string? value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, string.Empty);
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue(string id, out bool value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, false);
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetValue(string id, out byte value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = (byte)Preferences.Default.Get(InstancePrefix + id, 0);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out short value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = (short)Preferences.Default.Get(InstancePrefix + id, 0);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out ushort value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = (ushort)Preferences.Default.Get(InstancePrefix + id, 0);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out int value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, 0);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out uint value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = (uint)Preferences.Default.Get(InstancePrefix + id, 0L);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out long value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, 0L);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out ulong value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            return ulong.TryParse(Preferences.Default.Get(InstancePrefix + id, "0"), out value);
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out double value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, 0d);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out float value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, 0f);
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(string id, out DateTime value)
    {
        if (Preferences.Default.ContainsKey(InstancePrefix + id))
        {
            value = Preferences.Default.Get(InstancePrefix + id, DateTime.MinValue);
            return true;
        }

        value = DateTime.MinValue;
        return false;
    }

    public void RemoveValue(string id)
    {
        Preferences.Default.Remove(id);
    }
}
