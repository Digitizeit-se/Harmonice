using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Harmonize.Channel;
using Harmonize.Common.Models;
using Harmonize.Common.Services;
using Microsoft.Extensions.Logging;

namespace Harmonize.Worker;

/// <summary>
/// Base Class for all workers
/// </summary>
public abstract class BaseWorker : IBaseWorker
{
    /// <summary>
    /// The channel to read jobs passed from the previous worker
    /// <note>
    /// Not all workers will use an incoming channel.
    /// example: the first worker in the pipeline could get jobs from ftp, file system, etc
    /// </note>
    /// </summary>
    protected readonly IChannel<JobItem> IncomingJobs;

    /// <summary>
    /// The channel to write completed jobs to for the next worker to read
    /// <note>
    /// Not all workers will use an outgoing channel.
    /// example: the last worker in the pipeline could write jobs to a FTP, file system, etc
    /// </note>
    /// </summary>
    protected readonly IChannel<JobItem> CompletedJobs;

    /// <summary>
    /// A time trigger to run the worker on a schedule
    /// </summary>
    private readonly ITimeTrigger _timerTrigger;

    /// <summary>
    /// The logger for the worker
    /// </summary>
    private readonly ILogger<BaseWorker> _logger;

    /// <summary>
    /// The cancellation token source for the worker to cancel the worker
    /// TODO: Validate if this is needed
    /// or do we take a cancellation token from the creater
    /// </summary>
    private CancellationTokenSource _cts; 

    /// <summary>
    /// State of the worker <see cref="WorkerState"/>
    /// </summary>
    private WorkerState State { get; set; }


    /// <summary>
    /// When worker is created.
    /// </summary>
    public DateTime InitDateTime { get; set; }

    /// <summary>
    /// When worker last ran
    /// </summary>
    public DateTime LastRunDateTime { get; private set; }

    /// <summary>
    /// Configuration for the worker
    /// </summary>
    public Dictionary<string,string> WorkerSettings { get; set; }

    /// <summary>
    /// A unique id for the worker
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The order for the worker to run in the pipeline
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Description of what the worker does
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The given name of the worker
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The file name of the worker
    /// </summary>
    public string WorkerFileName { get; set; }
  
    /// <summary>
    /// The cron expression to run the worker on a schedule
    /// </summary>
    public string? CronExpression { get; set; }

    protected BaseWorker(
        IChannel<JobItem> incomingJobs,
        IChannel<JobItem> completedJobs,
        WorkerSettings workerSettings,
        IServiceProvider serviceProvider
        ) 
    {


    }



}
