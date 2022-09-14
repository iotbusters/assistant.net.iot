using Assistant.Net.Messaging.Exceptions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Exceptions;

/// <summary>
///     Requested resource wasn't found.
/// </summary>
public sealed class NotFoundException : MessageException
{
    /// <summary/>
    public NotFoundException() : base("Requested resource wasn't found.") { }

    /// <summary/>
    public NotFoundException(string? message) : base(message) { }

    /// <summary/>
    public NotFoundException(string? message, Exception? ex) : base(message, ex) { }
}
