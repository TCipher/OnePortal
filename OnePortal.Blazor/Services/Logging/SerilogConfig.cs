using Serilog;
using Serilog.Events;

namespace OnePortal.Blazor.Services.Logging
{
    public static class SerilogConfig
    {
        public static Serilog.ILogger CreateClientLogger() =>
            new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.BrowserConsole(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
    }
}
