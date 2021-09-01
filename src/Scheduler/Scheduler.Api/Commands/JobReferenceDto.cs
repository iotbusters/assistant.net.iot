using System;

namespace Assistant.Net.Scheduler.Api.Commands
{
    /// <summary>
    ///     Automation job reference.
    /// </summary>
    public class JobReferenceDto
    {
        /// <summary/>
        public JobReferenceDto(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }
    }
}