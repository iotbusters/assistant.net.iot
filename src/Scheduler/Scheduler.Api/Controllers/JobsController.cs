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

namespace Assistant.Net.Scheduler.Api.Controllers
{
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
        public Task<Guid> Create(JobCreateModel model, CancellationToken token) => model switch
        {
            {TriggerEventName: not null, TriggerEventMask: not null} =>
                client.Request(new JobTriggerCreateCommand(model.Name, model.TriggerEventName, model.TriggerEventMask), token),
            {Action: not null} =>
                client.Request(new JobActionCreateCommand(model.Name, model.Action), token),
            _ => throw new MessageContractException("Invalid payload.")
        };

        /// <summary>
        ///     Updates existing automation job by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique job id.</param>
        /// <param name="model">Update job details.</param>
        /// <param name="token" />
        [HttpPut("{id}")]
        public Task Update(Guid id, JobUpdateModel model, CancellationToken token) => model switch
        {
            {TriggerEventName: not null, TriggerEventMask: not null} =>
                client.Request(new JobTriggerUpdateCommand(id, model.Name, model.TriggerEventName, model.TriggerEventMask), token),
            {Action: not null} =>
                client.Request(new JobActionUpdateCommand(id, model.Name, model.Action), token),
            _ => throw new MessageContractException("Invalid payload.")
        };

        /// <summary>
        ///     Deletes existing automation job by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique job id.</param>
        /// <param name="token" />
        [HttpDelete("{id}")]
        public Task Delete(Guid id, CancellationToken token) =>
            client.Request(new JobDeleteCommand(id), token);
    }
}
