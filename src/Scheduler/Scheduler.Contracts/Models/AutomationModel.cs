﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation model.
/// </summary>
public sealed class AutomationModel
{
    /// <summary/>
    public AutomationModel(Guid id, string name, AutomationJobReferenceModel[] jobs)
    {
        Id = id;
        Name = name;
        Jobs = jobs.ToArray();
    }

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Name.
    /// </summary>
    [Required]
    public string Name { get; }

    /// <summary>
    ///     Job reference sequence.
    /// </summary>
    [Required]
    public AutomationJobReferenceModel[] Jobs { get; }
}