using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Api.Commands
{
    /// <summary>
    ///     Automation references query.
    /// </summary>
    public class AutomationReferencesQuery : IMessage<IEnumerable<AutomationReferenceModel>>
    {
    }
}