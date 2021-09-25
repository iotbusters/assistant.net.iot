using Assistant.Net.Scheduler.Contracts.Models;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation run update model.
    /// </summary>
    public class RunUpdateModel
    {
        /// <summary/>
        public RunUpdateModel(RunStatusDto status) =>
            Status = status;

        /// <summary>
        ///     Latest status of the run.
        /// </summary>
        public RunStatusDto Status { get; }
    }
}
