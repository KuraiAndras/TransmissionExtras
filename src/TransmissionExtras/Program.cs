using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using Serilog;
using Serilog.Events;

using TransmissionExtras;
using TransmissionExtras.Jobs;


var builder = Host.CreateApplicationBuilder(args);

var serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithDemystifiedStackTraces()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File(Path.Combine("logs", "log.log"), rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(serilogLogger);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddOptions<TransmissionOptions>().Bind(builder.Configuration.GetSection(TransmissionOptions.Section));
builder.Services.AddSingleton<IValidateOptions<TransmissionOptions>, ValidateTransmissionOptions>();

var jobs = await GetJobs();

builder.Services.AddQuartz(q =>
{
    if (jobs == null || jobs.Length == 0)
    {
        serilogLogger.Warning("No jobs found in jobs.json");
        return;
    }

    foreach (var torrentJobData in jobs)
    {
        q.AddTrigger(t => t
            .WithCronSchedule(torrentJobData.Cron)
            .WithIdentity($"{torrentJobData.Id}-cron")
            .ForJob(torrentJobData.Key.Value));

        if (torrentJobData.RunOnStartup == true)
        {
            q.AddTrigger(t => t
                .StartNow()
                .WithIdentity($"{torrentJobData.Id}-startup")
                .ForJob(torrentJobData.Key.Value));
        }

        q.AddJob(
            torrentJobData.HandlerType,
            torrentJobData.Key.Value,
            j => j
                .DisallowConcurrentExecution()
                .UsingJobData(new() { { TorrentJobData.JobDataKey, torrentJobData } }));
    }
});

builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    _ = app.Services.GetRequiredService<IOptions<TransmissionOptions>>().Value;
}
catch (OptionsValidationException e)
{
    LogSettingsValidationFailed(logger, e);
    return -2;
}

try
{
    await app.RunAsync();
}
catch (Exception e)
{
    LogRunningApplicationFailed(logger, e);
    return -3;
}

return 0;

static async Task<TorrentJobData[]?> GetJobs()
{
    await using var file = File.OpenRead("jobs.json");

    return await JsonSerializer.DeserializeAsync(file, TorrentJobJsonSerializerContext.Default.TorrentJobDataArray);
}

partial class Program
{
    [LoggerMessage(
        EventId = EventIds.Program.SettingsValidationFailed,
        Level = LogLevel.Critical)]
    static partial void LogSettingsValidationFailed(Microsoft.Extensions.Logging.ILogger logger, OptionsValidationException e);

    [LoggerMessage(
        EventId = EventIds.Program.RunningApplicationFailed,
        Level = LogLevel.Critical,
        Message = "Running the application has failed")]
    static partial void LogRunningApplicationFailed(Microsoft.Extensions.Logging.ILogger logger, Exception e);
}
