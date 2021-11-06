using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models
{
    /// <summary>
    ///     Automation run trigger reference model.
    /// </summary>
    public class TriggerReferenceModel
    {
        /// <summary/>
        public TriggerReferenceModel(Guid runId) =>
            RunId = runId;

        /// <summary>
        ///     Run id.
        /// </summary>
        [Required]
        public Guid RunId { get; set; }
    }
}
