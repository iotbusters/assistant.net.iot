using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation run reference model.
/// </summary>
public sealed class RunReferenceModel
{
    /// <summary/>
    public RunReferenceModel(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    [Required]
    public Guid Id { get; set; }
}
