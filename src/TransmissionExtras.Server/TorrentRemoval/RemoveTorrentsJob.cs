using Microsoft.Extensions.Options;

using Transmission.API.RPC.Entity;

namespace TransmissionExtras.Server.TorrentRemoval;

public sealed partial class RemoveTorrentsJob : BackgroundService
{
    private static readonly string[] RemoveTorrentFields = [TorrentFields.ID, TorrentFields.NAME, TorrentFields.SECONDS_SEEDING];

    private readonly IOptions<RemoveTorrentsOptions> _options;
    private readonly IOptions<TransmissionOptions> _transmissionOptions;
    private readonly ILogger<RemoveTorrentsJob> _logger;

    public RemoveTorrentsJob(IOptions<RemoveTorrentsOptions> options, ILogger<RemoveTorrentsJob> logger, IOptions<TransmissionOptions> transmissionOptions)
    {
        _options = options;
        _logger = logger;
        _transmissionOptions = transmissionOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var client = TransmissionClientFactory.GetClient(_transmissionOptions.Value);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var torrents = await client.TorrentGetAsync(RemoveTorrentFields);

                    var torrentsToRemove = torrents.Torrents
                        .Where(t => TimeSpan.FromSeconds(t.SecondsSeeding) >= _options.Value.RemoveAfter)
                        .ToArray();

                    if (!_options.Value.DryRun)
                    {
                        // TODO: make this awaitable
                        client.TorrentRemoveAsync(torrentsToRemove.Select(t => t.ID).ToArray(), _options.Value.DeleteData);
                    }
                    else
                    {
                        LogDryRun();
                    }

                    foreach (var torrent in torrentsToRemove)
                    {
                        LogRemovedTorrent(torrent.ID, torrent.Name, torrent.SecondsSeeding);
                    }

                    await Task.Delay(_options.Value.CheckInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    LogRemovingTorrentsJobStopping();

                    break;
                }
                catch (Exception e)
                {
                    LogRemoveTorrentsFailed(e);
                }
            }
        }
        catch (Exception e)
        {
            LogRemoveTorrentsJobFailed(e);
        }
    }

    [LoggerMessage(
        EventId = EventIds.RemoveTorrentsJob.RemovingTorrentsJobFailed,
        Level = LogLevel.Error,
        Message = "Removing torrents job failed. Stopping job.")]
    private partial void LogRemoveTorrentsJobFailed(Exception e);

    [LoggerMessage(
        EventId = EventIds.RemoveTorrentsJob.RemovingTorrentsFailed,
        Level = LogLevel.Error,
        Message = "Removing torrents failed")]
    private partial void LogRemoveTorrentsFailed(Exception e);

    [LoggerMessage(
        EventId = EventIds.RemoveTorrentsJob.DryRun,
        Level = LogLevel.Information,
        Message = "Dry run. Not actually removing torrents")]
    private partial void LogDryRun();

    [LoggerMessage(
        EventId = EventIds.RemoveTorrentsJob.RemovedTorrent,
        Level = LogLevel.Information,
        Message = "Removed torrent {id}, {name}, {secondsSeeding} seconds")]
    private partial void LogRemovedTorrent(int id, string name, int secondsSeeding);

    [LoggerMessage(
        EventId = EventIds.RemoveTorrentsJob.RemovingTorrentsJobStopping,
        Level = LogLevel.Information,
        Message = "Removing torrents job stopping")]
    private partial void LogRemovingTorrentsJobStopping();
}
