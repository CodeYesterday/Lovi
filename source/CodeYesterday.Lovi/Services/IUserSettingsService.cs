using System.Diagnostics.CodeAnalysis;

namespace CodeYesterday.Lovi.Services;

/// <summary>
/// Interface for services to persist user settings.
/// </summary>
[PublicAPI]
public interface IUserSettingsService
{
    /// <summary>
    /// Sets a new value.
    /// </summary>
    /// <typeparam name="T">The type of the value.
    /// Only <see cref="string"/>, <see cref="bool"/>, <see cref="byte"/>, <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>, <see cref="float"/>, <see cref="double"/> and <see cref="DateTime"/> are supported.</typeparam>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">The new value.</param>
    void SetValue<T>(string id, T value);

    /// <summary>
    /// Gets the persisted <see cref="string"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    string GetValue(string id, string defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="bool"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    bool GetValue(string id, bool defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="byte"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    byte GetValue(string id, byte defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="short"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    short GetValue(string id, short defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="ushort"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    ushort GetValue(string id, ushort defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="int"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    int GetValue(string id, int defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="uint"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    uint GetValue(string id, uint defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="long"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    long GetValue(string id, long defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="ulong"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    ulong GetValue(string id, ulong defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="double"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    double GetValue(string id, double defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="float"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    float GetValue(string id, float defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="DateTime"/> value or the <paramref name="defaultValue"/> if it does not exist.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>Returns the persisted value or <paramref name="defaultValue"/> if it does not exist.</returns>
    DateTime GetValue(string id, DateTime defaultValue);

    /// <summary>
    /// Gets the persisted <see cref="string"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or <see langword="null"/> otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Gets the persisted <see cref="bool"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or <see langword="false"/> otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out bool value);

    /// <summary>
    /// Gets the persisted <see cref="byte"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out byte value);

    /// <summary>
    /// Gets the persisted <see cref="short"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out short value);

    /// <summary>
    /// Gets the persisted <see cref="ushort"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out ushort value);

    /// <summary>
    /// Gets the persisted <see cref="int"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out int value);

    /// <summary>
    /// Gets the persisted <see cref="uint"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out uint value);

    /// <summary>
    /// Gets the persisted <see cref="long"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out long value);

    /// <summary>
    /// Gets the persisted <see cref="ulong"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out ulong value);

    /// <summary>
    /// Gets the persisted <see cref="double"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out double value);

    /// <summary>
    /// Gets the persisted <see cref="float"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or 0 otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out float value);

    /// <summary>
    /// Gets the persisted <see cref="DateTime"/> value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">When this method returns contains the persisted value if it exists or <see cref="DateTime.MinValue"/> otherwise.</param>
    /// <returns>Returns <see langword="true"/> if the value exists, <see langword="false"/> otherwise.</returns>
    bool TryGetValue(string id, out DateTime value);

    /// <summary>
    /// Removes the value if it exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    void RemoveValue(string id);
}

/// <summary>
/// Typed interface for services to persist user settings.
/// </summary>
/// <typeparam name="TInstance">The type of the instantiating object. The <see cref="Type.Name"/> is used as a prefix for all value IDs.</typeparam>
[PublicAPI]
public interface IUserSettingsService<TInstance> : IUserSettingsService
{ }