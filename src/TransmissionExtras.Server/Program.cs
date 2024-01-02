using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using TransmissionExtras.Server;
using TransmissionExtras.Server.TorrentRemoval;

var builder = WebApplication.CreateSlimBuilder(args);

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

var app = builder.Build();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
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

partial class Program
{
    [LoggerMessage(
        EventId = EventIds.Program.SettingsValidationFailed,
        Level = LogLevel.Critical)]
    static partial void LogSettingsValidationFailed(ILogger logger, OptionsValidationException e);
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

[JsonSerializable(typeof(WeatherForecast[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
