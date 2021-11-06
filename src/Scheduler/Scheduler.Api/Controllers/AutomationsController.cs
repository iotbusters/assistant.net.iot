using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Controllers
{
    /// <summary>
    ///     Automation operations controller.
    /// </summary>
    [Route("automations")]
    [ApiController]
    public class AutomationsController
    {
        private readonly IMessagingClient client;

        /// <summary/>
        public AutomationsController(IMessagingClient client) =>
            this.client = client;

        /// <summary>
        ///     Gets all available automations.
        ///     It includes only minimal set of properties.
        /// </summary>
        /// <param name="token" />
        /// <returns>Automation light preview sequence.</returns>
        [HttpGet]
        public async Task<IEnumerable<AutomationReferenceModel>> Get(CancellationToken token) =>
            await client.Request(new AutomationReferencesQuery(), token);

        /// <summary>
        ///     Gets specific automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        /// <param name="token" />
        /// <returns>Automation object.</returns>
        [HttpGet("{id}")]
        public async Task<AutomationModel> Get(Guid id, CancellationToken token) =>
            await client.Request(new AutomationQuery(id), token);

        /// <summary>
        ///     Defines new automation.
        /// </summary>
        /// <param name="model">Create automation details.</param>
        /// <param name="token" />
        /// <returns>Location header with reference to the new  automation.</returns>
        [HttpPost]
        public async Task<Guid> Create(AutomationCreateModel model, CancellationToken token) =>
            await client.Request(new AutomationCreateCommand(model.Name, model.Jobs.Select(x => new JobReferenceDto(x.Id)).ToArray()), token);

        /// <summary>
        ///     Updates existing automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        /// <param name="model">Update automation details.</param>
        /// <param name="token" />
        [HttpPut("{id}")]
        public async Task Update(Guid id, AutomationUpdateModel model, CancellationToken token) =>
            await client.Request(new AutomationUpdateCommand(id, model.Name, model.Jobs.Select(x => new JobReferenceDto(x.Id)).ToArray()), token);

        /// <summary>
        ///     Deletes existing automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        /// <param name="token" />
        [HttpDelete("{id}")]
        public async Task Delete(Guid id, CancellationToken token) =>
            await client.Request(new AutomationDeleteCommand(id), token);
    }
}
