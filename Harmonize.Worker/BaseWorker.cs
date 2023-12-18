using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Channels;
using Harmonize.Channel;
using Harmonize.Common.Models;
using Harmonize.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;

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
    protected IChannel<JobItem> IncomingJobs;

    /// <summary>
    /// The channel to write completed jobs to for the next worker to read
    /// <note>
    /// Not all workers will use an outgoing channel.
    /// example: the last worker in the pipeline could write jobs to a FTP, file system, etc
    /// </note>
    /// </summary>
    protected  IChannel<JobItem> CompletedJobs;

    /// <summary>
    /// A time trigger to run the worker on a schedule
    /// </summary>
    private  ITimeTrigger _timerTrigger;

    /// <summary>
    /// The logger for the worker
    /// </summary>
    private ILogger<BaseWorker>? _logger;

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
    public DateTime InitDateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// When worker changed to started state
    /// </summary>
    public DateTime StartDateTiem { get; set; }

    /// <summary>
    /// When worker last ran
    /// </summary>
    public DateTime LastRunDateTime { get; private set; } = default;

    /// <summary>
    /// Configuration for the worker
    /// </summary>
    public Dictionary<string,string> WorkerConfiguration { get; set; }

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

    /// <summary>
    /// The action to run when work is taken from the queue
    /// </summary>
    public Func<JobItem, Task>? WorkAction { get; set; }

    /// <summary>
    /// Local token to cancel operations in the worker
    /// </summary>
    private CancellationToken Token { get; set; } = default;

    private Task ConsumeQueTask;

    protected BaseWorker(
        IChannel<JobItem> incomingJobs,
        IChannel<JobItem> completedJobs,
        WorkerSettings workerSettings,
        IServiceProvider services,
        Func<JobItem, Task>? workAction = null) 
    {
        IncomingJobs = incomingJobs;
        CompletedJobs = completedJobs;
        Order = workerSettings.Order;
        Description = workerSettings.Description;
        CronExpression = workerSettings.CronExpression;
        WorkerFileName = workerSettings.FileName;
        WorkerConfiguration = workerSettings.Arguments;
        _logger = services.GetService<ILogger<BaseWorker>>() ?? new NullLogger<BaseWorker>();
        _timerTrigger = services.GetRequiredService<ITimeTrigger>();
        State = WorkerState.Init;
        WorkAction = workAction;
        _cts = new CancellationTokenSource();
       
    }

    /// <summary>
    /// Get how long the worker has been running
    /// </summary>
    /// <returns><see cref="TimeSpan"/> since status changed to started</returns>
    public TimeSpan GetRunningTime()
    {
        return DateTime.Now - StartDateTiem;
    }

    /// <summary>
    /// Number of jobs waiting to be processed
    /// </summary>
    /// <returns><see cref="int"/> number of jobs on queue</returns>
    public int ItemsInJobQueue()
    {
        return IncomingJobs.Count();
    }

    public virtual async Task StartAsync(Func<Task> action)
    {

    }

    /// <summary>
    /// Add a job to the queue for the next worker to process
    /// </summary>
    /// <param name="job"></param>
    /// <returns>a Task to await for the operation to finish</returns>
    public virtual async Task EnqueueAsync(JobItem job)
    {
        _logger.LogDebug($"worker: {Name} Enqueueing job");
        await CompletedJobs.WriteAsync(job, Token);
    }

    public virtual async Task StartAsync()
    {
        //If running do nothing.
        if (State == WorkerState.Running) return;
        
        try
        {
            _logger.LogInformation($"worker: {Name} Id: {Id} Is entering Running state.");

            //Start consuming the queue on its own thread
            ConsumeQueTask = Task.Run(async () =>
            {
                await ConsumeQueue(_cts.Token);
            }, _cts.Token);
            
            State =  WorkerState.Running;
            StartDateTiem = DateTime.Now;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// Consume the incoming jobs queue and calls the worker action to process the jobitem
    /// This will run until the token is canceled.
    /// </summary>
    /// <param name="token"><see cref="CancellationToken"/> to cancel this operation</param>
    /// <returns>a Task to await for this operation to finish</returns>
    public virtual async Task ConsumeQueue(CancellationToken token)
    {
        await foreach (var (jobItem, ackNum) in IncomingJobs.ReadAllAsync(token))
        {
            if (WorkAction == null) continue;
            
            //Call work action to process the jobitem
            await WorkAction(jobItem).ConfigureAwait(false);
            if (IncomingJobs.NeedAck())
            {
                IncomingJobs.Ack(ackNum);
            }
        }
    }

    /// <summary>
    /// Stop all work in the worker
    /// TODO: This need to stop listening on the channel
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual async Task<bool> Stop()
    {
        _logger!.LogDebug($"worker: {Name} with Id: {Id} Got the stop sign.");

        //Call cancel on to all tokens from source
        await _cts.CancelAsync();

        //Wait for the consume queue task to finish
        while (ConsumeQueTask.Status != TaskStatus.Canceled 
               && ConsumeQueTask.Status != TaskStatus.Faulted
               && ConsumeQueTask.Status != TaskStatus.RanToCompletion)
        {
            _logger.LogDebug($"Worker: {Name} waiting for the right task status.");

            await Task.Delay(1000).ConfigureAwait(false);

        }
       

        State = WorkerState.Stopped;
      
        return true;
    }


}
