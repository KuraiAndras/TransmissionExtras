using System.Text.Json.Serialization;

using Quartz;

namespace TransmissionExtras.Jobs;

[JsonSourceGenerationOptions
(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
)]
[JsonSerializable(typeof(TorrentJobData[]))]
public partial class TorrentJobJsonSerializerContext : JsonSerializerContext { }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "id")]
public abstract partial class TorrentJobData
{
    public const string JobDataKey = "data";

    protected TorrentJobData() => Key = new(() => new($"{Id}-job", "torrent-job"));

    [JsonIgnore]
    public abstract string Id { get; }

    [JsonIgnore]
    public abstract Type HandlerType { get; }

    [JsonIgnore]
    public Lazy<JobKey> Key { get; }

    public required string Cron { get; init; }
    public bool DryRun { get; init; }
    public bool? RunOnStartup { get; init; }
}
