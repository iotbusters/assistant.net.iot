using Assistant.Net.Scheduler.Contracts.Enums;

namespace Assistant.Net.Scheduler.Contracts.Models
{
    /// <summary>
    ///     Automation run status.
    /// </summary>
    public class RunStatusDto
    {
        /// <summary/>
        public RunStatusDto(RunStatus value) : this(value, null)
        {
        }

        /// <summary/>
        public RunStatusDto(RunStatus value, string? message)
        {
            Value = value;
            Message = message;
        }

        /// <summary>
        ///     Status value of the run.
        /// </summary>
        public RunStatus Value { get; }

        /// <summary>
        ///     Message describing the status.
        /// </summary>
        public string? Message { get; }
    }
}
