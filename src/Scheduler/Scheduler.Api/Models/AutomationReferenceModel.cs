using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation reference model.
    /// </summary>
    public class AutomationReferenceModel
    {
        /// <summary/>
        public AutomationReferenceModel(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        [Required]
        public Guid Id { get; set; }
    }
}