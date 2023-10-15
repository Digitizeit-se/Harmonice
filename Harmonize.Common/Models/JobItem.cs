using Newtonsoft.Json;

namespace Harmonize.Common.Models;
public readonly struct JobItem
{
    /// <summary>
    /// Name of who created the job
    /// </summary>
    public string? CreatorName { get; }

    /// <summary>
    /// Id of who created the job
    /// </summary>
    public Guid CreatorId { get; }

    /// <summary>
    /// Job object as string, if jobType is object this is json string
    /// </summary>
    public string Job { get; }

    /// <summary>
    /// Object type of the job stored in Job
    /// </summary>
    public string JobType { get; }

    /// <summary>
    /// Constructor to create new JobItem
    /// </summary>
    /// <param name="job">The job object</param>
    /// <param name="creatorName">Name of the creator</param>
    /// <param name="creatorId">Id of the creator</param>
    public JobItem(object job, string? creatorName, Guid creatorId)
    {
        JobType = job.GetType().Name;

        Job = JobType == "String" ? (string)job : JsonConvert.SerializeObject(job,Formatting.None);

        CreatorName = creatorName;
        CreatorId = creatorId;
    }

    /// <summary>
    /// Converts the job to the specified type
    /// </summary>
    /// <typeparam name="T">specified type </typeparam>
    /// <returns>Job object converted to type of T </returns>
    public T? GetJob<T>()
    {
        return JobType == "String" ? (T)Convert.ChangeType(Job, typeof(T)) : JsonConvert.DeserializeObject<T>(Job);
    }
}
