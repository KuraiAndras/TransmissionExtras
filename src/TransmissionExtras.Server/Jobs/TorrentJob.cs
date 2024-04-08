﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Quartz;

using Transmission.API.RPC;

namespace TransmissionExtras.Server.Jobs;

public abstract partial class TorrentJob<TData, TSelf> : IJob where TData : TorrentJobData
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

        LogStartingJob(Logger, context.JobDetail.Key.Name, context.Trigger.Key.Name);

        try
        {
            await Execute((TData)context.MergedJobDataMap[TorrentJobData.JobDataKey], TransmissionClientFactory.GetClient(Options.Value), context.CancellationToken);
        }
        catch (TaskCanceledException e)
        {
            LogCancelledJob(Logger, e, context.JobDetail.Key.Name);
            await ScheduleRetry(context);
        }
        catch (Exception e)
        {
            LogJobFailed(Logger, e, context.JobDetail.Key.Name);
            await ScheduleRetry(context);
        }
    }

    private async ValueTask ScheduleRetry(IJobExecutionContext context)
    {
        var oldTrigger = context.Trigger;

        var newTrigger = TriggerBuilder.Create()
            .WithIdentity($"{oldTrigger.Key.Name}-retry", oldTrigger.Key.Group)
            .StartAt(DateTimeOffset.UtcNow.Add(Options.Value.RetryTimeout))
            .Build();

        await context.Scheduler.ScheduleJob(newTrigger);
    }

    protected abstract Task Execute(TData data, Client client, CancellationToken cancellationToken);

    [LoggerMessage(
        EventId = EventIds.TorrentJob.StartingJob,
        Level = LogLevel.Information,
        Message = "Starting job {key} on trigger {trigger}")]
    private static partial void LogStartingJob(ILogger logger, string key, string trigger);

    [LoggerMessage(
        EventId = EventIds.TorrentJob.CancelledJob,
        Level = LogLevel.Warning,
        Message = "Cancelled job {key}")]
    private static partial void LogCancelledJob(ILogger logger, TaskCanceledException e, string key);

    [LoggerMessage(
        EventId = EventIds.TorrentJob.JobFailed,
        Level = LogLevel.Warning,
        Message = "Job {key} failed")]
    private static partial void LogJobFailed(ILogger logger, Exception e, string key);
}
