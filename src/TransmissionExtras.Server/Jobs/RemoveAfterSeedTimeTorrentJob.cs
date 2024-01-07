using System.Text.Json.Serialization;

namespace TransmissionExtras.Server.Jobs;

public sealed class RemoveAfterSeedTimeTorrentJobData : TorrentJobData
{
    public const string JobName = "remove-after-seed-time";

    public override string Id => JobName;

    public override Type HandlerType => typeof(RemoveAfterSeedTimeTorrentJob);

    public required TimeSpan After { get; init; }
    public bool DeleteData { get; init; }
}

[JsonDerivedType(typeof(RemoveAfterSeedTimeTorrentJobData), RemoveAfterSeedTimeTorrentJobData.JobName)]
partial class TorrentJobData { }

public sealed class RemoveAfterSeedTimeTorrentJob : TorrentJob<RemoveAfterSeedTimeTorrentJobData, RemoveAfterSeedTimeTorrentJob>
{
    public RemoveAfterSeedTimeTorrentJob(ILogger<RemoveAfterSeedTimeTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(RemoveAfterSeedTimeTorrentJobData data)
    {
    }
}
