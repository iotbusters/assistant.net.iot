﻿using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation creation command.
/// </summary>
public class AutomationCreateCommand : IMessage<Guid>, IMessageCacheIgnored
{
    /// <summary/>
    public AutomationCreateCommand(string name, JobReferenceDto[] jobs)
    {
        Name = name;
        Jobs = jobs;
    }

    /// <summary>
    ///     Name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Job reference sequence.
    /// </summary>
    public JobReferenceDto[] Jobs { get; }
}
