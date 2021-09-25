﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Handlers
{
    internal class JobQueryHandler : IMessageHandler<JobQuery, JobModel>
    {
        private readonly IStorage<Guid, JobModel> storage;

        public JobQueryHandler(IStorage<Guid, JobModel> storage) =>
            this.storage = storage;

        public async Task<JobModel> Handle(JobQuery command, CancellationToken token) =>
            await storage.GetOrDefault(command.Id, token) ?? throw new NotFoundException();
    }
}
