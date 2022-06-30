using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job create model abstraction.
/// </summary>
public abstract class JobCreateModel
{
    /// <summary>
    ///     Name.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
}
