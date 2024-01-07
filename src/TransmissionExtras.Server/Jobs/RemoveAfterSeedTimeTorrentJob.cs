using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

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

public sealed partial class RemoveAfterSeedTimeTorrentJob : TorrentJob<RemoveAfterSeedTimeTorrentJobData, RemoveAfterSeedTimeTorrentJob>
{
    private static readonly string[] RemoveTorrentFields = [TorrentFields.ID, TorrentFields.NAME, TorrentFields.SECONDS_SEEDING];

    public RemoveAfterSeedTimeTorrentJob(ILogger<RemoveAfterSeedTimeTorrentJob> logger, IOptions<TransmissionOptions> options) : base(logger, options)
    {
    }

    protected override async Task Execute(RemoveAfterSeedTimeTorrentJobData data, Client client, CancellationToken cancellationToken)
    {
        var torrents = await client.TorrentGetAsync(RemoveTorrentFields);

        var torrentsToRemove = torrents.Torrents
            .Where(t => TimeSpan.FromSeconds(t.SecondsSeeding) >= data.After)
            .ToArray();

        if (!data.DryRun)
        {
            // TODO: make this awaitable
            client.TorrentRemoveAsync(torrentsToRemove.Select(t => t.ID).ToArray(), data.DeleteData);
        }
        else
        {
            LogDryRun(Logger);
        }

        foreach (var torrent in torrentsToRemove)
        {
            LogRemovedTorrent(Logger, torrent.ID, torrent.Name, torrent.SecondsSeeding);
        }
    }

    [LoggerMessage(
        EventId = EventIds.RemoveAfterSeedTimeTorrentJob.DryRun,
        Level = LogLevel.Information,
        Message = "Dry run. Not actually removing torrents")]
    private static partial void LogDryRun(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.RemoveAfterSeedTimeTorrentJob.RemovedTorrent,
        Level = LogLevel.Information,
        Message = "Removed torrent after seed time: {id}, {name}, {secondsSeeding} seconds")]
    private static partial void LogRemovedTorrent(ILogger logger, int id, string name, int secondsSeeding);
}
