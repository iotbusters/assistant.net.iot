using Assistant.Net.Messaging.Exceptions;
using System;

namespace Assistant.Net.Scheduler.Api.Exceptions
{
    /// <summary>
    ///     The queried data wasn't found.
    /// </summary>
    public class NotFoundException : MessageException
    {
        /// <summary/>
        public NotFoundException() : base("Resource wasn't found.") { }

        /// <summary/>
        public NotFoundException(string? message) : base(message) { }

        /// <summary/>
        public NotFoundException(string? message, Exception? ex) : base(message, ex) { }
    }
}