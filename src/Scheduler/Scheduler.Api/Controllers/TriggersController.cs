using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Controllers;

/// <summary>
///     Automation run trigger operations controller.
/// </summary>
[Route("triggers")]
[ApiController]
public sealed class TriggersController
{
    private readonly IMessagingClient client;

    /// <summary/>
    public TriggersController(IMessagingClient client) =>
        this.client = client;

    /// <summary>
    ///     Gets all available triggers.
    ///     It includes only minimal set of properties.
    /// </summary>
    /// <param name="token" />
    /// <returns>Trigger light preview sequence.</returns>
    [HttpGet]
    public Task<IEnumerable<TriggerReferenceModel>> Get(CancellationToken token) =>
        client.Request(new TriggerReferencesQuery(), token);

    /// <summary>
    ///     Gets specific trigger.
    /// </summary>
    /// <param name="runId">Run id.</param>
    /// <param name="token" />
    /// <returns>Trigger object.</returns>
    [HttpGet("{runId}")]
    public Task<TriggerModel> Get(Guid runId, CancellationToken token) =>
        client.Request(new TriggerQuery(runId), token);
}
