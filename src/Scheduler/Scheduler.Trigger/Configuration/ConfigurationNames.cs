using System;

namespace Assistant.Net.Scheduler.Trigger.Configuration
{
    /// <summary>
    ///     Configuration names.
    /// </summary>
    public static class ConfigurationNames
    {
        /// <summary>
        ///     Trigger section name.
        /// </summary>
        [Obsolete("replaced with TriggerEventSource")]
        public const string TriggerSectionName = "Trigger";

        /// <summary>
        ///     Remote messaging section name.
        /// </summary>
        public const string Messaging = "RemoteMessageHandling";
    }
}
