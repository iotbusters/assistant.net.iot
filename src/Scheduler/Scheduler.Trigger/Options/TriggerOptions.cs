using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options
{
    /// <summary>
    ///     Trigger configuration options used for event listening.
    /// </summary>
    public class TriggerOptions
    {
        /// <summary>
        ///     Event names.
        /// </summary>
        [Required, MinLength(1)]
        public TriggerEvent[] Events { get; set; } = null!;
    }
}
