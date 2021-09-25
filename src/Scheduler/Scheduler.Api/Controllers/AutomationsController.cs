using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns>Automation light preview sequence.</returns>
        [HttpGet]
        public async Task<IEnumerable<AutomationReferenceModel>> Get() =>
            await client.SendAs(new AutomationReferencesQuery());

        /// <summary>
        ///     Gets specific automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        /// <returns>Automation object.</returns>
        [HttpGet("{id}")]
        public async Task<AutomationModel> Get(Guid id) =>
            await client.SendAs(new AutomationQuery(id));

        /// <summary>
        ///     Defines new automation.
        /// </summary>
        /// <param name="model">Create automation details.</param>
        /// <returns>Location header with reference to the new  automation.</returns>
        [HttpPost]
        public async Task<Guid> Create(AutomationCreateModel model) =>
            await client.SendAs(new AutomationCreateCommand(model.Name, model.Jobs.Select(x => new JobReferenceDto(x.Id))));

        /// <summary>
        ///     Updates existing automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        /// <param name="model">Update automation details.</param>
        [HttpPut("{id}")]
        public async Task Update(Guid id, AutomationUpdateModel model) =>
            await client.SendAs(new AutomationUpdateCommand(id, model.Name, model.Jobs.Select(x => new JobReferenceDto(x.Id))));

        /// <summary>
        ///     Deletes existing automation by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique automation id.</param>
        [HttpDelete("{id}")]
        public async Task Delete(Guid id) =>
            await client.SendAs(new AutomationDeleteCommand(id));
    }
}
