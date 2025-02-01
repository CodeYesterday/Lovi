namespace CodeYesterday.Lovi.Session;

/// <summary>
/// Interface for storing property values.
/// </summary>
[PublicAPI]
public interface IPropertyStorage
{
    /// <summary>
    /// Saves the properties into the storage.
    /// </summary>
    /// <param name="sessionDirectory">The session directory.</param>
    /// <param name="dataDirectory">The data directory of the session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveProperties(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the properties from storage.
    /// </summary>
    /// <param name="sessionDirectory">The session directory.</param>
    /// <param name="dataDirectory">The data directory of the session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LoadProperties(string sessionDirectory, string dataDirectory, CancellationToken cancellationToken);

    /// <summary>
    /// Set the property value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">The value to store.</param>
    void SetPropertyValue<TValue>(string id, TValue value);

    /// <summary>
    /// Gets the property value. If the value is not found the <paramref name="defaultValue"/> is returned.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="id">The ID of the value.</param>
    /// <param name="defaultValue">The default value.</param>
    TValue GetPropertyValue<TValue>(string id, TValue defaultValue);

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="id">The ID of the value.</param>
    /// <param name="value">The value to store. Is never <see langword="null"/> if the return value is <see langword="true"/>.</param>
    /// <returns>Returns <see langword="true"/> if the value was found.</returns>
    bool TryGetPropertyValue<TValue>(string id, out TValue? value);

    /// <summary>
    /// Removes the value.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    void RemovePropertyValue(string id);

    /// <summary>
    /// Returns if the value exists.
    /// </summary>
    /// <param name="id">The ID of the value.</param>
    /// <returns>Returns <see langword="true"/> if the value exists.</returns>
    bool PropertyValueExists(string id);
}
