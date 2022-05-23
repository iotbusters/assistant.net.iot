using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation job model base.
/// </summary>
public abstract class JobModel
{
    /// <summary/>
    protected JobModel(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Job name.
    /// </summary>
    [Required]
    public string Name { get; }
}