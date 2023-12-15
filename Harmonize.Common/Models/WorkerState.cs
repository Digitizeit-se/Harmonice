namespace Harmonize.Common.Models;
public enum WorkerState
{
    /// <summary>
    /// When worker is created it is in the init state
    /// </summary>
    Init,
    
    /// <summary>
    /// When worker is running it is in the running state
    /// </summary>
    Running,
    
    /// <summary>
    /// When worker is paused it is in the paused state
    /// </summary>
    Paused,
    
    /// <summary>
    /// When worker is stopped it is in the stopped state
    /// </summary>
    Failed,
    
    /// <summary>
    /// When worker is stopped it is in the stopped state
    /// </summary>
    Stopped
}
