namespace Volte.Entities;

public struct LogMessage
{
    public LogSeverity Severity { get; private set; }
    public LogSource Source { get; private set; }
    public string Message { get; private set; }
    public Exception Exception { get; private set; }

    public static LogMessage FromDiscordLogMessage(DiscordLogMessage message)
        => new()
        {
            Message = message.Message,
            Severity = message.Severity,
            Exception = message.Exception,
            Source = LogSources.Parse(message.Source)
        };
}