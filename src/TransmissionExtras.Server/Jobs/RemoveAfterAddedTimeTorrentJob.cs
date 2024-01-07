using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Transmission.API.RPC;

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
    public RemoveAfterAddedTimeTorrentJob(ILogger<RemoveAfterAddedTimeTorrentJob> logger, IOptions<TransmissionOptions> options) : base(logger, options)
    {
    }

    protected override async Task Execute(RemoveAfterAddedTimeTorrentJobData data, Client client, CancellationToken cancellationToken)
    {
    }
}
