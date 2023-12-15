namespace Harmonize.Common.Models;


/// <summary>
/// TODO: This is a Place holder class  for the settings of a worker manager
/// </summary>
public class WorkerManagerSettings : IEquatable<WorkerManagerSettings>
{
    /// <summary>
    /// Configuration for the channel used by the worker manager
    /// </summary>
    public ChannelSettings ChannelSettings { get; set; } 

    /// <summary>
    /// Configuration for the workers used by the worker manager
    /// </summary>
    public List<WorkerSettings> WorkerSettings { get; set; }

    //TODO: Add more settings for the worker manager











    public bool Equals(WorkerManagerSettings? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
       return false;
    }

    public override bool Equals(object obj)
    {
        return obj is null ? false :
            ReferenceEquals(this, obj) ? true : obj.GetType() == this.GetType() && Equals((WorkerManagerSettings)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine("");
    }
}
