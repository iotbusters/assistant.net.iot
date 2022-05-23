namespace Assistant.Net.Scheduler.Contracts;

/// <summary>
///     Configuration names.
/// </summary>
public static class ConfigurationNames
{
    /// <summary>
    ///     Section name of messaging connection string.
    /// </summary>
    public const string Messaging = "RemoteMessageHandling";

    /// <summary>
    ///     Section name of storage connection name.
    /// </summary>
    public const string Database = "StorageDatabase";

    /// <summary>
    ///     Section name of trigger polling name.
    /// </summary>
    public const string TriggerPolling = "TriggerPolling";
}