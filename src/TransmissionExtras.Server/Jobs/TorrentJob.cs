using System.Text.Json;

using Quartz;

namespace TransmissionExtras.Server.Jobs;

public abstract class TorrentJob<TData, TSelf> : IJob where TData : TorrentJobData
{
    protected TorrentJob(ILogger<TSelf> logger)
    {
        Logger = logger;
    }

    protected ILogger<TSelf> Logger { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        Logger.LogInformation("Starting job {key} on trigger {trigger}", context.JobDetail.Key.Name, context.Trigger.Key.Name);

        await Execute((TData)context.MergedJobDataMap[TorrentJobData.JobDataKey]);
    }

    protected abstract Task Execute(TData data);
}
