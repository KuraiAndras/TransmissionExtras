using Microsoft.Extensions.Options;

using Transmission.API.RPC.Entity;

namespace TransmissionExtras.Server.TorrentVerification;

public sealed partial class VerifyTorrentsJob : BackgroundService
{
    private static readonly string[] VerifyTorrentFields = [TorrentFields.ID, TorrentFields.NAME, TorrentFields.ERROR_STRING, TorrentFields.ERROR];

    private readonly IOptions<VerifyTorrentsOptions> _options;
    private readonly IOptions<TransmissionOptions> _transmissionOptions;
    private readonly ILogger<VerifyTorrentsJob> _logger;

    public VerifyTorrentsJob(IOptions<VerifyTorrentsOptions> options, IOptions<TransmissionOptions> transmissionOptions, ILogger<VerifyTorrentsJob> logger)
    {
        _options = options;
        _transmissionOptions = transmissionOptions;
        _logger = logger;
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
                    var torrents = await client.TorrentGetAsync(VerifyTorrentFields);

                    var torrentsToVerify = torrents.Torrents
                        .Where(t => t.Error == 3)
                        .ToArray();

                    if (!_options.Value.DryRun)
                    {
                        // TODO: make this awaitable
                        // TODO: use int ids
                        client.TorrentVerifyAsync(torrentsToVerify.Select(t => t.ID as object).ToArray());
                    }
                    else
                    {
                        LogDryRun();
                    }

                    foreach (var torrent in torrentsToVerify)
                    {
                        LogVerifyingTorrent(torrent.ID, torrent.Name, torrent.ErrorString);
                    }

                    await Task.Delay(_options.Value.CheckInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    LogVerifyTorrentsJobStopping();

                    break;
                }
                catch (Exception ex)
                {
                    LogVerifyingTorrentsJobsFailed(ex);
                }
            }
        }
        catch (Exception e)
        {
            LogVerifyTorrentsJobFailed(e);
        }
    }

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentsJob.VerifyTorrentsJobFailed,
        Level = LogLevel.Error,
        Message = "Verifying torrents job failed. Stopping job.")]
    private partial void LogVerifyTorrentsJobFailed(Exception e);

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentsJob.VerifyingTorrentsFailed,
        Level = LogLevel.Error,
        Message = "Verifying torrents failed")]
    private partial void LogVerifyingTorrentsJobsFailed(Exception e);

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentsJob.DryRun,
        Level = LogLevel.Information,
        Message = "Dry run. Not actually verifying torrents")]
    private partial void LogDryRun();

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentsJob.VerifyingTorrent,
        Level = LogLevel.Information,
        Message = "Verifying torrent {id}, {name}, {error}")]
    private partial void LogVerifyingTorrent(int id, string name, string error);

    [LoggerMessage(
        EventId = EventIds.VerifyTorrentsJob.VerifyTorrentsJobStopping,
        Level = LogLevel.Information,
        Message = "Verify torrents job stopping")]
    private partial void LogVerifyTorrentsJobStopping();
}
