using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation reference model.
/// </summary>
public class AutomationReferenceModel
{
    /// <summary/>
    public AutomationReferenceModel(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    [Required]
    public Guid Id { get; set; }
}