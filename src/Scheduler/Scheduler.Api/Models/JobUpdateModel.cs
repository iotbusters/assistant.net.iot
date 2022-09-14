using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job update model.
/// </summary>
public sealed class JobUpdateModel
{
    /// <summary>
    ///     Name.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
}
