using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Transmission.API.RPC;

namespace TransmissionExtras.Server.Jobs;

public sealed class VerifyTorrentJobData : TorrentJobData
{
    public const string JobName = "verify";

    public override string Id => JobName;

    public override Type HandlerType => typeof(VerifyTorrentJob);
}

[JsonDerivedType(typeof(VerifyTorrentJobData), VerifyTorrentJobData.JobName)]
partial class TorrentJobData { }

public sealed class VerifyTorrentJob : TorrentJob<VerifyTorrentJobData, VerifyTorrentJob>
{
    public VerifyTorrentJob(ILogger<VerifyTorrentJob> logger, IOptions<TransmissionOptions> options) : base(logger, options)
    {
    }

    protected override async Task Execute(VerifyTorrentJobData data, Client client, CancellationToken cancellationToken)
    {
    }
}
