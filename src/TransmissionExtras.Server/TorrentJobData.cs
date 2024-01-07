using System.Text.Json.Serialization;

using Quartz;

namespace TransmissionExtras.Server;

[JsonSourceGenerationOptions
(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true
)]
[JsonSerializable(typeof(TorrentJobData[]))]
public partial class TorrentJobJsonSerializerContext : JsonSerializerContext { }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "id")]
[JsonDerivedType(typeof(RemoveAfterSeedTimeTorrentJobData), RemoveAfterSeedTimeTorrentJobData.JobName)]
[JsonDerivedType(typeof(RemoveAfterAddedTimeTorrentJobData), RemoveAfterAddedTimeTorrentJobData.JobName)]
[JsonDerivedType(typeof(VerifyTorrentJobData), VerifyTorrentJobData.JobName)]
public abstract class TorrentJobData
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
}

public sealed class RemoveAfterSeedTimeTorrentJobData : TorrentJobData
{
    public const string JobName = "remove-after-seed-time";

    public override string Id => JobName;

    public override Type HandlerType => typeof(RemoveAfterSeedTimeTorrentJob);

    public required TimeSpan After { get; init; }
    public bool DeleteData { get; init; }
}

public sealed class RemoveAfterAddedTimeTorrentJobData : TorrentJobData
{
    public const string JobName = "remove-after-added-time";

    public override string Id => JobName;
    public override Type HandlerType => typeof(RemoveAfterAddedTimeTorrentJob);

    public required TimeSpan After { get; init; }
    public bool DeleteData { get; init; }
}

public sealed class VerifyTorrentJobData : TorrentJobData
{
    public const string JobName = "verify";

    public override string Id => JobName;

    public override Type HandlerType => typeof(VerifyTorrentJob);
}

public abstract class TorrentJob<TData, TSelf> : IJob where TData : TorrentJobData
{
    protected TorrentJob(ILogger<TSelf> logger)
    {
        Logger = logger;
    }

    protected ILogger<TSelf> Logger { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        var data = (TData)context.MergedJobDataMap[TorrentJobData.JobDataKey];

        Logger.LogInformation("Starting job {JobId}", data.Id);

        await Execute(data);
    }

    protected abstract Task Execute(TData data);
}

public sealed class RemoveAfterSeedTimeTorrentJob : TorrentJob<RemoveAfterSeedTimeTorrentJobData, RemoveAfterSeedTimeTorrentJob>
{
    public RemoveAfterSeedTimeTorrentJob(ILogger<RemoveAfterSeedTimeTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(RemoveAfterSeedTimeTorrentJobData data)
    {
    }
}

public sealed class RemoveAfterAddedTimeTorrentJob : TorrentJob<RemoveAfterAddedTimeTorrentJobData, RemoveAfterAddedTimeTorrentJob>
{
    public RemoveAfterAddedTimeTorrentJob(ILogger<RemoveAfterAddedTimeTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(RemoveAfterAddedTimeTorrentJobData data)
    {
    }
}

public sealed class VerifyTorrentJob : TorrentJob<VerifyTorrentJobData, VerifyTorrentJob>
{
    public VerifyTorrentJob(ILogger<VerifyTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(VerifyTorrentJobData data)
    {
    }
}
