using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Scheduler.Trigger.Configuration
{
    /// <summary>
    ///     Configuration builder extension methods for trigger scheduler.
    /// </summary>

    [Obsolete("replaced with TriggerEventSource")]
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        ///     Trigger section configuration from existing triggers.
        /// </summary>
        /// <param name="builder" />
        /// <param name="connectionString">MongoDB connection string.</param>
        /// <param name="databaseName">MongoDB database.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddTrigger(this IConfigurationBuilder builder, string connectionString, string databaseName)
        {
            builder.Add(new TriggerConfigurationSource(connectionString, databaseName));
            return builder;
        }
    }
}
