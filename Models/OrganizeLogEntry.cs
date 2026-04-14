using System;

namespace Nook.Models;

public enum LogLevel { Info, Success, Warning, Error }

public class OrganizeLogEntry
{
    public string Message { get; init; } = string.Empty;
    public LogLevel Level { get; init; } = LogLevel.Info;
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Icon => Level switch
    {
        LogLevel.Success => "✓",
        LogLevel.Warning => "⚠",
        LogLevel.Error   => "✗",
        _                => "›"
    };
    public string Color => Level switch
    {
        LogLevel.Success => "#43D9A2",
        LogLevel.Warning => "#FF9F43",
        LogLevel.Error   => "#FF6584",
        _                => "#94a3b8"
    };
}
