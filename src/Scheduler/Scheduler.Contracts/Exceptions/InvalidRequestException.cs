using Assistant.Net.Messaging.Exceptions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Exceptions;

/// <summary>
///     Requested operation was invalid.
/// </summary>
public class InvalidRequestException : MessageException
{
    /// <summary/>
    public InvalidRequestException() : base("Requested operation was invalid.") { }

    /// <summary/>
    public InvalidRequestException(string? message) : base(message) { }

    /// <summary/>
    public InvalidRequestException(string? message, Exception? ex) : base(message, ex) { }
}
