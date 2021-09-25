using Assistant.Net.Scheduler.Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation create model.
    /// </summary>
    public class AutomationCreateModel
    {
        /// <summary>
        ///     Name.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        ///     Job reference sequence.
        /// </summary>
        [Required]
        public AutomationJobReferenceModel[] Jobs { get; set; } = null!;
    }
}
