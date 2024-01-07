using Microsoft.Extensions.Options;

using Quartz;

using Transmission.API.RPC;

namespace TransmissionExtras.Server.Jobs;

public abstract class TorrentJob<TData, TSelf> : IJob where TData : TorrentJobData
{
    protected TorrentJob(ILogger<TSelf> logger, IOptions<TransmissionOptions> options)
    {
        Logger = logger;
        Options = options;
    }

    protected ILogger<TSelf> Logger { get; }
    protected IOptions<TransmissionOptions> Options { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        Logger.LogInformation("Starting job {key} on trigger {trigger}", context.JobDetail.Key.Name, context.Trigger.Key.Name);

        try
        {
            await Execute((TData)context.MergedJobDataMap[TorrentJobData.JobDataKey], TransmissionClientFactory.GetClient(Options.Value), context.CancellationToken);
        }
        catch (TaskCanceledException e)
        {
            Logger.LogInformation(e, "Cancelled job {key}", context.JobDetail.Key.Name);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Job {key} failed", context.JobDetail.Key.Name);
        }
    }

    protected abstract Task Execute(TData data, Client client, CancellationToken cancellationToken);
}
