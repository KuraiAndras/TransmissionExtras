using System.Text.Json.Serialization;

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
    public VerifyTorrentJob(ILogger<VerifyTorrentJob> logger) : base(logger)
    {
    }

    protected override async Task Execute(VerifyTorrentJobData data)
    {
    }
}
