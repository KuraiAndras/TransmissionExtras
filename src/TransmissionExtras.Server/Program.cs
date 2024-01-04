using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Events;

using TransmissionExtras.Server;
using TransmissionExtras.Server.TorrentRemoval;
using TransmissionExtras.Server.TorrentVerification;

var logFilePath = Path.Combine("logs", "log.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithDemystifiedStackTraces()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Day), blockWhenFull: true)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithDemystifiedStackTraces()
        .WriteTo.Console()
        .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Day)));

    // Add services to the container.

    builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

    builder.Services
        .AddOptions<TransmissionOptions>()
        .Bind(builder.Configuration.GetSection(TransmissionOptions.Section));
    builder.Services
        .AddSingleton<IValidateOptions<TransmissionOptions>, ValidateTransmissionOptions>();

    builder.Services
        .AddOptions<RemoveTorrentsOptions>()
        .Bind(builder.Configuration.GetSection(RemoveTorrentsOptions.Section));
    builder.Services
        .AddSingleton<IValidateOptions<RemoveTorrentsOptions>, ValidateRemoveTorrentsOptions>();
    builder.Services.AddHostedService<RemoveTorrentsJob>();

    builder.Services
        .AddOptions<VerifyTorrentsOptions>()
        .Bind(builder.Configuration.GetSection(VerifyTorrentsOptions.Section));
    builder.Services.AddHostedService<VerifyTorrentsJob>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapGet("api/healthcheck", async Task<Results<Ok<HealthCheckResult>, ProblemHttpResult>> ([FromServices] IOptions<TransmissionOptions> options) =>
    {
        try
        {
            _ = app.Services.GetRequiredService<IOptions<TransmissionOptions>>().Value;
            _ = app.Services.GetRequiredService<IOptions<RemoveTorrentsOptions>>().Value;

            var client = TransmissionClientFactory.GetClient(options.Value);

            _ = await client.GetSessionInformationAsync();

            return TypedResults.Ok<HealthCheckResult>(new("Ok"));
        }
        catch (Exception ex)
        {
            LogHealthCheckFailed(app.Logger, ex);

            return TypedResults.Problem(title: "Healthchecks failed", detail: ex.Message);
        }
    });

    try
    {
        _ = app.Services.GetRequiredService<IOptions<TransmissionOptions>>().Value;
        _ = app.Services.GetRequiredService<IOptions<RemoveTorrentsOptions>>().Value;
    }
    catch (OptionsValidationException e)
    {
        LogSettingsValidationFailed(app.Logger, e);
        return -1;
    }

    await app.RunAsync();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");

    return -2;
}
finally
{
    await Log.CloseAndFlushAsync();
}

partial class Program
{
    [LoggerMessage(
        EventId = EventIds.Program.SettingsValidationFailed,
        Level = LogLevel.Critical)]
    static partial void LogSettingsValidationFailed(Microsoft.Extensions.Logging.ILogger logger, OptionsValidationException e);

    [LoggerMessage(
        EventId = EventIds.Program.HealthCheckFailed,
        Level = LogLevel.Critical)]
    static partial void LogHealthCheckFailed(Microsoft.Extensions.Logging.ILogger logger, Exception e);
}

record HealthCheckResult(string Status);

[JsonSerializable(typeof(HealthCheckResult))]

internal partial class AppJsonSerializerContext : JsonSerializerContext { }
