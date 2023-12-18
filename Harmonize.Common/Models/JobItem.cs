using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using ProtoBuf;
using ProtoBuf.Serializers;

namespace Harmonize.Common.Models;

[Serializable]
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
    private string Job { get; }

    private byte[] Data { get; }

    public bool IsBinary { get; }

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
    public JobItem(object job, string? creatorName, Guid creatorId, bool storeAsBinary = false)
    {
        JobType = job.GetType().Name;
        CreatorName = creatorName;
        CreatorId = creatorId;


        if (storeAsBinary)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, job);
            Data = stream.ToArray();
            return;
        }

        Job = JobType == "String" ? (string)job : JsonConvert.SerializeObject(job,Formatting.None);

    }

    /// <summary>
    /// Converts the job to the specified type
    /// </summary>
    /// <typeparam name="T">specified type </typeparam>
    /// <returns>Job object converted to type of T </returns>
    public T? GetJob<T>()
    {
        //not binary then it's a string as string or object as json string
        if (!IsBinary)
            return JobType == "String" ? (T)Convert.ChangeType(Job, typeof(T)) : JsonConvert.DeserializeObject<T>(Job);
        
        //Data is binary so we need to deserialize it
        using var stream = new MemoryStream();

        // Ensure that our stream is at the beginning.
        stream.Write(Data, 0, Data.Length);
        stream.Seek(0, SeekOrigin.Begin);

        return Serializer.Deserialize<T>(stream);
    }

    public string GetJob()
    {
        return Job;
    }

}
