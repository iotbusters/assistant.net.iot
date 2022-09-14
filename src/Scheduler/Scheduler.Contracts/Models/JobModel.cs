using Assistant.Net.Scheduler.Contracts.Commands;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation job model.
/// </summary>
public sealed class JobModel
{
    /// <summary/>
    public JobModel(Guid id, string name, JobConfigurationDto configuration)
    {
        Id = id;
        Name = name;
        Configuration = configuration;
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

    /// <summary>
    ///     Specific job configuration.
    /// </summary>
    public JobConfigurationDto Configuration { get; }
}
