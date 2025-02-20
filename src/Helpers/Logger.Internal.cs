﻿using Colorful;
using Sentry.Extensibility;

using Color = System.Drawing.Color;
using Optional = Gommon.Optional;

namespace Volte.Helpers;

public static partial class Logger
{
    public class SentryTranslator : IDiagnosticLogger
    {
        public bool IsEnabled(SentryLevel logLevel) 
            => Version.IsDevelopment || logLevel is not SentryLevel.Debug;
            
        public void Log(SentryLevel logLevel, string message, Exception exception = null, params object[] args)
            => LogSync.Lock(() =>
            {
                var (color, value) = VerifySentryLevel(logLevel);
                Append($"{value}:".P(), color);
                Append("[SENTRY]".P(), Color.Chartreuse);

                if (!message.IsNullOrWhitespace())
                    Append(message.Format(args), Color.White);

                if (exception != null)
                {
                    var toWrite = $"{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}";
                    Append(toWrite, Color.IndianRed);
                }
                    
                Console.Write(Environment.NewLine);
            });
    }

    private static readonly string[] VolteAscii =
        new Figlet().ToAscii("Volte").ConcreteValue.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
    static Logger() => FilePath.Logs.Create();
        
    private static readonly object LogSync = new();
    
    internal static void PrintHeader()
    {
        Info(LogSource.Volte, MessageService.Separator.Trim());
        VolteAscii.ForEach(ln => Info(LogSource.Volte, ln));
        Info(LogSource.Volte, MessageService.Separator.Trim());
        Info(LogSource.Volte, $"Currently running Volte V{Version.InformationVersion}.");
    }

    private const string Side = "----------------------------------------------------------";
    private static bool _logFileNoticePrinted;

    internal static void LogFileRestartNotice()
    {
        if (_logFileNoticePrinted || !(Config.EnabledFeatures?.LogToFile ?? false)) return;
            
        GetRelevantLogPath().AppendAllText($"{Side}RESTARTING{Side}\n");
            
        _logFileNoticePrinted = true;
    }
    
    private static void Log<TData>(LogSeverity s, LogSource from, string message, Exception e, InvocationInfo<TData> caller = default)
    {
        if (s is LogSeverity.Debug && !Config.DebugEnabled)
            return;
        
        LogSync.Lock(() => Execute(s, from, message, e, caller));
    }
    
    private static void Execute<TData>(LogSeverity s, LogSource src, string message, Exception e, InvocationInfo<TData> caller)
    {
        var content = new StringBuilder();

        if (IsDebugLoggingEnabled && caller.IsInitialized)
        {
            var debugInfo = Optional.None<string>();
            switch (caller)
            {
                case InvocationInfo<FullDebugInfo> fdi:
                    debugInfo =
                        $"{fdi.GetSourceFileName()}:{fdi.Data.SourceFileLocation.LineInFile}->{fdi.Data.CallerName}";
                    break;
                
                case InvocationInfo<SourceMemberName> smn:
                    debugInfo = smn.Data.Value;
                    break;
                
                case InvocationInfo<SourceFileLocation> fdi:
                    debugInfo = $"{fdi.GetSourceFileName()}:{fdi.Data.LineInFile}";
                    break;
            }

            debugInfo.IfPresent(debugInfoContent =>
            {
                Append(debugInfoContent, Color.Aquamarine, ref content);
                Append(" |>  ", Color.Goldenrod, ref content);
            });
        }
        
        var (color, value) = VerifySeverity(s);
        Append($"{value}:".P(), color);
        var dt = DateTime.Now.ToLocalTime();
        content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {value} -> ");

        (color, value) = VerifySource(src);
        Append($"[{value}]".P(), color);
        content.Append(string.Intern($"{value} -> "));

        if (!message.IsNullOrWhitespace())
            Append(message, Color.White, ref content);

        if (e != null)
        {
            e.SentryCapture(scope => 
                scope.AddBreadcrumb("This exception might not have been thrown, and may not be important; it is merely being logged."));
            Append(Environment.NewLine + (e.Message.IsNullOrEmpty() ? "No message provided" : e.Message) + 
                   Environment.NewLine + e.StackTrace, 
                Color.IndianRed, ref content);
        }

        if (Environment.NewLine != content[^1].ToString())
        {
            Console.Write(Environment.NewLine);
            content.AppendLine();
        }
            
        if (Config.EnabledFeatures?.LogToFile ?? false)
            GetRelevantLogPath().AppendAllText(content.ToString().TrimEnd('\n').Append("\n"));
    }

    private static FilePath GetLogFilePath(DateTime date) => new FilePath("logs") / string.Intern($"{date.Month}-{date.Day}-{date.Year}.log");

    private static FilePath GetRelevantLogPath() => GetLogFilePath(DateTime.Now);

    private static void Append(string m, Color c)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
    }

    private static void Append(string m, Color c, ref StringBuilder sb)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
        sb?.Append(m);
    }

    private static (Color Color, string Source) VerifySource(LogSource source) =>
        source switch
        {
            LogSource.Discord => (Color.RoyalBlue, "DISCORD"),
            LogSource.Gateway => (Color.RoyalBlue, "DISCORD"),
            LogSource.Volte => (Color.LawnGreen, "CORE"),
            LogSource.Service => (Color.Gold, "SERVICE"),
            LogSource.Module => (Color.LimeGreen, "MODULE"),
            LogSource.Rest => (Color.Red, "REST"),
            LogSource.Unknown => (Color.Fuchsia, "UNKNOWN"),
            LogSource.Sentry => (Color.Chartreuse, "SENTRY"),
            LogSource.UI => (Color.Teal, "UI"),
            _ => throw new InvalidOperationException($"The specified LogSource {source} is invalid.")
        };
        
    private static (Color Color, string Level) VerifySentryLevel(SentryLevel level) =>
        level switch
        {
            SentryLevel.Debug => (Color.RoyalBlue, "DEBUG"),
            SentryLevel.Info => (Color.RoyalBlue, "INFO"),
            SentryLevel.Warning => (Color.LawnGreen, "WARN"),
            SentryLevel.Error => (Color.Gold, "ERROR"),
            SentryLevel.Fatal => (Color.LimeGreen, "FATAL"),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };


    private static (Color Color, string Level) VerifySeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => (Color.Maroon, "CRITICAL"),
            LogSeverity.Error => (Color.DarkRed, "ERROR"),
            LogSeverity.Warning => (Color.Yellow, "WARN"),
            LogSeverity.Info => (Color.SpringGreen, "INFO"),
            LogSeverity.Verbose => (Color.Pink, "VERBOSE"),
            LogSeverity.Debug => (Color.SandyBrown, "DEBUG"),
            _ => throw new InvalidOperationException($"The specified LogSeverity ({severity}) is invalid.")
        };

    private static string P(this string input) => string.Intern(input.PadRight(10));
}