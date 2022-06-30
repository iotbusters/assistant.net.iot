using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Queries;

/// <summary>
///     Automation references query.
/// </summary>
public class AutomationReferencesQuery : IMessage<IEnumerable<AutomationReferenceModel>>, INonCaching { }
