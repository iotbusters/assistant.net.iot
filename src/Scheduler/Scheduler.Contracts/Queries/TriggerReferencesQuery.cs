using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Queries;

/// <summary>
///     Automation run trigger references query.
/// </summary>
public sealed class TriggerReferencesQuery : IMessage<IEnumerable<TriggerReferenceModel>>, INonCaching { }
