using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job reference model.
    /// </summary>
    public class AutomationJobReferenceModel
    {
        /// <summary/>
        public AutomationJobReferenceModel(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        [Required]
        public Guid Id { get; set; }
    }
}