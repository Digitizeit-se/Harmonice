    namespace Harmonize.Common.Models;


/// <summary>
/// TODO: This is a Place holder class  for the settings of a worker
/// </summary>
public class WorkerSettings
{
    public int Order { get; set; }
    public string? Description { get; set; }
    public string? Name { get; set; }
    public string? CronExpression { get; set; }
    public required string FileName { get; set; }
    public Dictionary<string,string> Arguments { get; set; } = new Dictionary<string,string>();
}
