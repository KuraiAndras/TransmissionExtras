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
        var data = (TData)context.MergedJobDataMap[TorrentJobData.JobDataKey];

        Logger.LogInformation("Starting job {JobId}", data.Id);

        await Execute(data);
    }

    protected abstract Task Execute(TData data);
}
