using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Controllers;

/// <summary>
///     Automation job operations controller.
/// </summary>
[Route("jobs")]
[ApiController]
public class JobsController
{
    private readonly IMessagingClient client;

    /// <summary/>
    public JobsController(IMessagingClient client) =>
        this.client = client;

    /// <summary>
    ///     Gets specific automation job by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Unique job id.</param>
    /// <param name="token" />
    /// <returns>Automation job object.</returns>
    [HttpGet("{id}")]
    public Task<JobModel> Get(Guid id, CancellationToken token) =>
        client.Request(new JobQuery(id), token);

    /// <summary>
    ///     Defines new automation job.
    /// </summary>
    /// <param name="model">Create job details.</param>
    /// <param name="token" />
    /// <returns>Location header with reference to the new automation job.</returns>
    [HttpPost]
    public Task<Guid> Create(JobCreateModel model, CancellationToken token)
    {
        JobConfigurationDto configuration = model switch
        {
            JobEventCreateModel {EventName: not null, EventMask: not null} x =>
                new JobEventConfigurationDto(x.EventName, x.EventMask),
            JobActionCreateModel {Action: not null} x =>
                new JobActionConfigurationDto(x.Action),
            JobDailyTimerCreateModel x =>
                new JobDailyTimerConfigurationDto(x.Time, x.Days),
            JobStopwatchTimerCreateModel x =>
                new JobStopwatchTimerConfigurationDto(x.Time),
            _ => throw new MessageContractException("Invalid payload.")
        };
        return client.Request(new JobCreateCommand(model.Name, configuration), token);
    }

    /// <summary>
    ///     Updates existing automation job by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Unique job id.</param>
    /// <param name="model">Update job details.</param>
    /// <param name="token" />
    [HttpPut("{id}")]
    public async Task Update(Guid id, JobUpdateModel model, CancellationToken token)
    {
        // todo: add optimistic concurrency
        var job = await client.Request(new JobQuery(id), token);
        await client.Request(new JobUpdateCommand(id, model.Name, job.Configuration), token);
    }

    /// <summary>
    ///     Deletes existing automation job by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Unique job id.</param>
    /// <param name="token" />
    [HttpDelete("{id}")]
    public Task Delete(Guid id, CancellationToken token) =>
        client.Request(new JobDeleteCommand(id), token);
}
