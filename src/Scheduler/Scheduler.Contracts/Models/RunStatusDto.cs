using Assistant.Net.Scheduler.Contracts.Enums;

namespace Assistant.Net.Scheduler.Contracts.Models
{
    /// <summary>
    ///     Automation run status.
    /// </summary>
    public class RunStatusDto
    {
        /// <summary>
        ///     Serialization purpose only.
        /// </summary>
        public RunStatusDto() { }

        /// <summary/>
        public RunStatusDto(RunStatus status, string? message = null)
        {
            Value = status;
            Message = message;
        }

        /// <summary>
        ///     Status value of the run.
        /// </summary>
        public RunStatus Value { get; set; }

        /// <summary>
        ///     Message describing the status.
        /// </summary>
        public string? Message { get; set; }
    }
}
