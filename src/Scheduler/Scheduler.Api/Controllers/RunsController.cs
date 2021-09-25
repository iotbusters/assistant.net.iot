using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Controllers
{
    /// <summary>
    ///     Automation run operations controller.
    /// </summary>
    [Route("runs")]
    [ApiController]
    public class RunsController
    {
        private readonly IMessagingClient client;

        /// <summary/>
        public RunsController(IMessagingClient client) =>
            this.client = client;

        /// <summary>
        ///     Gets specific automation run.
        /// </summary>
        /// <param name="id">Run id.</param>
        /// <returns>Automation run object.</returns>
        [HttpGet("{id}")]
        public Task<RunModel> Get(Guid id) =>
            client.SendAs(new RunQuery(id));

        /// <summary>
        ///     Defines new automation run sequence from an automation jobs.
        /// </summary>
        /// <param name="model">Create run details.</param>
        /// <returns>Location header with reference to the first run from the sequence.</returns>
        [HttpPost]
        public Task<Guid> Create(RunCreateModel model) =>
            client.SendAs(new RunCreateCommand(model.AutomationId));

        /// <summary>
        ///     Updates automation run.
        /// </summary>
        /// <param name="id">Run id.</param>
        /// <param name="model">Update run details.</param>
        [HttpPut("{id}")]
        public Task Update(Guid id, RunUpdateModel model) =>
            client.SendAs(new RunUpdateCommand(id, model.Status));

        /// <summary>
        ///     Deletes automation run cascadly.
        /// </summary>
        /// <param name="id">Run id.</param>
        [HttpDelete("{id}")]
        public Task Delete(Guid id) =>
            client.SendAs(new RunDeleteCommand(id));
    }
}
