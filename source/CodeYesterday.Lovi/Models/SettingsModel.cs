using Radzen;
using Serilog.Events;

namespace CodeYesterday.Lovi.Models;

/// <summary>
/// The model for all application settings.
/// </summary>
public class SettingsModel
{
    /// <summary>
    /// The model for log level settings.
    /// </summary>
    public class LogLevelModel
    {
        /// <summary>
        /// Gets or sets the icon for the log level.
        /// </summary>
        public required string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color for the log level.
        /// </summary>
        public required string Color { get; set; }

        /// <summary>
        /// Gets or sets the contrast color for the log level.
        /// </summary>
        public string? ContrastColor { get; set; }
    }

    /// <summary>
    /// Gets an array with all log levels sorted from <see cref="LogEventLevel.Fatal"/> to <see cref="LogEventLevel.Verbose"/>.
    /// </summary>
    public LogEventLevel[] LogLevels { get; }

    /// <summary>
    /// Gets the dictionary with all log level settings.
    /// </summary>
    public IDictionary<LogEventLevel, LogLevelModel> LogLevelSettings { get; } = new Dictionary<LogEventLevel, LogLevelModel>();

    /// <summary>
    /// Gets the default <see cref="TooltipOptions"/>.
    /// </summary>
    public TooltipOptions TooltipOptions { get; } = new()
    {
        Delay = 1000,
        Style = "background-color: var(--rz-base-background-color); border: 1px solid var(--rz-text-color); color: var(--rz-text-color);"
    };

    /// <summary>
    /// Gets or set the timestamp format.
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// Gets the timestamp format string.
    /// </summary>
    public string TimestampFormatString => $"{{0:{TimestampFormat}}}";

    /// <summary>
    /// Creates a new instance of the settings model with default values.
    /// </summary>
    public SettingsModel()
    {
        LogLevels = Enum.GetValues<LogEventLevel>().OrderByDescending(l => (int)l).ToArray();

        // TODO: Set te secondary colors which are used for checkbox hovering to a slightly brighter color.
        LogLevelSettings.Add(LogEventLevel.Fatal, new()
        {
            Icon = "crisis_alert",
            Color = "mediumvioletred"
        });

        LogLevelSettings.Add(LogEventLevel.Error, new()
        {
            Icon = "error",
            Color = "red"
        });

        LogLevelSettings.Add(LogEventLevel.Warning, new()
        {
            Icon = "warning",
            Color = "orange",
            ContrastColor = "black"
        });

        LogLevelSettings.Add(LogEventLevel.Information, new()
        {
            Icon = "info",
            Color = "steelblue"
        });

        LogLevelSettings.Add(LogEventLevel.Debug, new()
        {
            Icon = "adb",
            Color = "darkgray",
            ContrastColor = "black"
        });

        LogLevelSettings.Add(LogEventLevel.Verbose, new()
        {
            Icon = "density_small",
            Color = "lightsalmon",
            ContrastColor = "black"
        });
    }

    /// <summary>
    /// Returns the settings for a log level.
    /// </summary>
    /// <param name="logLevel">The <see cref="LogEventLevel"/> to get the settings for.</param>
    /// <returns>Returns the settings for the specified <paramref name="logLevel"/>.</returns>
    /// <exception cref="ArgumentException">The <paramref name="logLevel"/> has an invalid value.</exception>
    public LogLevelModel GetLogLevelSettings(LogEventLevel logLevel)
    {
        if (LogLevelSettings.TryGetValue(logLevel, out var settings)) return settings;

        throw new ArgumentException($"logLevel {logLevel} is invalid", nameof(logLevel));
    }
}
