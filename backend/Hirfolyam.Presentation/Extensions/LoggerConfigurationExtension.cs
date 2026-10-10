using Serilog;
using Serilog.Events;

namespace Hirfolyam.Presentation.Extensions;

public static class LoggerConfigurationExtension
{
    private const string ConsoleOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";

    public static LoggerConfiguration WriteToApplicationConsole(this LoggerConfiguration logging)
    {
        return logging.WriteTo.Console(outputTemplate: ConsoleOutputTemplate, standardErrorFromLevel: LogEventLevel.Error);
    }
}
