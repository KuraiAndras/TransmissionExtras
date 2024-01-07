using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Transmission.API.RPC;

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
    public RemoveAfterSeedTimeTorrentJob(ILogger<RemoveAfterSeedTimeTorrentJob> logger, IOptions<TransmissionOptions> options) : base(logger, options)
    {
    }

    protected override async Task Execute(RemoveAfterSeedTimeTorrentJobData data, Client client, CancellationToken cancellationToken)
    {

    }
}
