using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
        /// <returns>Automation job object.</returns>
        [HttpGet("{id}")]
        public Task<JobModel> Get(Guid id)  => client.SendAs(
            new JobQuery(id));

        /// <summary>
        ///     Defines new automation job.
        /// </summary>
        /// <param name="model">Create job details.</param>
        /// <returns>Location header.</returns>
        [HttpPost]
        public Task<Guid> Create(JobCreateModel model) => client.SendAs(
            new JobCreateCommand(model.Name, model.Trigger, model.TriggerEventMask, model.Type, model.Parameters));
        // -- sample 1
        // listens <= { EventType:DeviceCustomEvent, PayloadMask:{ DeviceId:XXX }, PropertyMask:{ device-type:Sensor, sensor-type:Temperature } }
        // sends => JobRunCompletedEvent { Status:Success, Properties:{ } }
        // -- sample 2
        // listens <= { EventType:TimerTriggeredEvent, PayloadMask:null, PropertyMask:null }
        // sends => JobRunCompletedEvent { Status:Success, Properties:{ } }
        // -- sample 3
        // listens <= { EventType:JobRunCompletedEvent, PayloadMask:null, PropertyMask:null }
        // sends => JobRunCompletedEvent { Status:Success, Properties:{ } }

        /// <summary>
        ///     Updates existing automation job by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique job id.</param>
        /// <param name="model">Update job details.</param>
        [HttpPut("{id}")]
        public Task Update(Guid id, JobUpdateModel model) => client.SendAs(
            new JobUpdateCommand(id, model.Name, model.Trigger, model.TriggerEventMask, model.Type, model.Parameters));

        /// <summary>
        ///     Deletes existing automation job by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique job id.</param>
        [HttpDelete("{id}")]
        public Task Delete(Guid id) => client.SendAs(
            new JobDeleteCommand(id));
    }
}