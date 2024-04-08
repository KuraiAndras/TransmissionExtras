using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

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

public sealed partial class RemoveAfterAddedTimeTorrentJob : TorrentJob<RemoveAfterAddedTimeTorrentJobData, RemoveAfterAddedTimeTorrentJob>
{
    private static readonly string[] RemoveTorrentFields = [TorrentFields.ID, TorrentFields.NAME, TorrentFields.ADDED_DATE];

    public RemoveAfterAddedTimeTorrentJob(
        ILogger<RemoveAfterAddedTimeTorrentJob> logger,
        IOptions<TransmissionOptions> options,
        TimeProvider timeProvider)
        : base(logger, options, timeProvider) { }

    protected override async Task Execute(RemoveAfterAddedTimeTorrentJobData data, Client client, CancellationToken cancellationToken)
    {
        var torrents = await client.TorrentGetAsync(RemoveTorrentFields);

        var now = Time.GetLocalNow();

        var torrentsToRemove = torrents.Torrents
            .Where(t => DateTimeOffset.FromUnixTimeSeconds(t.AddedDate) + data.After <= now)
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
            LogRemovedTorrent(Logger, torrent.ID, torrent.Name, DateTimeOffset.FromUnixTimeSeconds(torrent.AddedDate) + data.After);
        }
    }

    [LoggerMessage(
        EventId = EventIds.RemoveAfterAddedTimeTorrentJob.DryRun,
        Level = LogLevel.Information,
        Message = "Dry run. Not actually removing torrents")]
    private static partial void LogDryRun(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.RemoveAfterAddedTimeTorrentJob.RemovedTorrent,
        Level = LogLevel.Information,
        Message = "Removed torrent after added date: {id}, {name}, remove after {removeDate}")]
    private static partial void LogRemovedTorrent(ILogger logger, int id, string name, DateTimeOffset removeDate);
}
