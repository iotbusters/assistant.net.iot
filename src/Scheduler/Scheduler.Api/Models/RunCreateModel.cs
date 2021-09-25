using System;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation run job create model.
    /// </summary>
    public class RunCreateModel
    {
        /// <summary/>
        public RunCreateModel(Guid automationId) =>
            AutomationId = automationId;

        /// <summary>
        ///     Related automation id.
        /// </summary>
        public Guid AutomationId { get; }
    }
}
