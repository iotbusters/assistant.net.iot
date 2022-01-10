using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;

namespace Assistant.Net.Scheduler.Contracts.Queries
{
    /// <summary>
    ///     Automation run trigger references query.
    /// </summary>
    public class TriggerReferencesQuery : IMessage<TriggerReferenceModel[]> { }
}
