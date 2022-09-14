using Assistant.Net.Scheduler.Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation update model.
/// </summary>
public sealed class AutomationUpdateModel
{
    /// <summary>
    ///     Name.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Job reference sequence.
    /// </summary>
    [Required]
    public AutomationJobReferenceModel[] Jobs { get; set; } = null!;
}