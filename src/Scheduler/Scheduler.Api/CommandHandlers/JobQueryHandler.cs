using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Commands;
using Assistant.Net.Scheduler.Api.Exceptions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.CommandHandlers
{
    internal class JobQueryHandler : IMessageHandler<JobQuery, JobModel>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public JobQueryHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public Task<JobModel> Handle(JobQuery command) =>
            storage.GetOrDefault(command.Id).MapSuccess(x => x ?? throw new NotFoundException());
    }
}