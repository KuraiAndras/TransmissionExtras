using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace TransmissionExtras.Server.Jobs;

public sealed class VerifyTorrentJobData : TorrentJobData
{
    public const string JobName = "verify";

    public override string Id => JobName;

    public override Type HandlerType => typeof(VerifyTorrentJob);
}

[JsonDerivedType(typeof(VerifyTorrentJobData), VerifyTorrentJobData.JobName)]
partial class TorrentJobData { }

public sealed partial class VerifyTorrentJob : TorrentJob<VerifyTorrentJobData, VerifyTorrentJob>
{
    private static readonly string[] VerifyTorrentFields = [TorrentFields.ID, TorrentFields.NAME, TorrentFields.ERROR_STRING, TorrentFields.ERROR];

    public VerifyTorrentJob(
        ILogger<VerifyTorrentJob> logger,
        IOptions<TransmissionOptions> options,
        TimeProvider timeProvider)
        : base(logger, options, timeProvider) { }

    protected override async Task Execute(VerifyTorrentJobData data, Client client, CancellationToken cancellationToken)
    {
        var torrents = await client.TorrentGetAsync(VerifyTorrentFields);

        var torrentsToVerify = torrents.Torrents
            .Where(t => t.Error == 3)
            .ToArray();

        if (!data.DryRun)
        {
            // TODO: make this awaitable
            // TODO: use int ids
            client.TorrentVerifyAsync(torrentsToVerify.Select(t => t.ID as object).ToArray());
        }
        else
        {
            LogDryRun(Logger);
        }

        foreach (var torrent in torrentsToVerify)
        {
            LogVerifyingTorrent(Logger, torrent.ID, torrent.Name, torrent.ErrorString);
        }
    }

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentJob.DryRun,
        Level = LogLevel.Information,
        Message = "Dry run. Not actually verifying torrents")]
    private static partial void LogDryRun(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentJob.VerifyingTorrent,
        Level = LogLevel.Information,
        Message = "Verifying torrent {id}, {name}, {error}")]
    private static partial void LogVerifyingTorrent(ILogger logger, int id, string name, string error);
}
