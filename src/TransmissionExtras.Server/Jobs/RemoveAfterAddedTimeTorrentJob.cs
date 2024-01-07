using System.Text.Json.Serialization;

namespace TransmissionExtras.Server.Jobs;

public sealed class RemoveAfterAddedTimeTorrentJobData : TorrentJobData
{
    public const string JobName = "remove-after-added-time";

    public override string Id => JobName;
    public override Type HandlerType => typeof(RemoveAfterAddedTimeTorrentJob);

    public required TimeSpan After { get; init; }
    public bool DeleteData { get; init; }
}

[JsonDerivedType(typeof(RemoveAfterAddedTimeTorrentJobData), RemoveAfterAddedTimeTorrentJobData.JobName)]
partial class TorrentJobData { }

public sealed class RemoveAfterAddedTimeTorrentJob : TorrentJob<RemoveAfterAddedTimeTorrentJobData, RemoveAfterAddedTimeTorrentJob>
{
    public RemoveAfterAddedTimeTorrentJob(ILogger<RemoveAfterAddedTimeTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(RemoveAfterAddedTimeTorrentJobData data)
    {
    }
}
