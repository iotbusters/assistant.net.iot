﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Events;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Tests.RemoteHandlers
{
    public class TimerTriggeredEventHandlerTests
    {
        [Test]
        public async Task Handle_TimerTriggeredEventHandler_delegatesEvent()
        {
            var run = new RunModel(
                id: Guid.NewGuid(),
                nextRunId: Guid.NewGuid(),
                automationId: Guid.NewGuid(),
                jobSnapshot: new JobTriggerModel(
                    id: Guid.NewGuid(),
                    name: "name",
                    triggerEventName: "Event",
                    triggerEventMask: new Dictionary<string, string>()));
            var handler = new TestMessageHandler<TimerTriggeredEvent, None>(new None());
            using var fixture = new SchedulerRemoteHandlerFixtureBuilder()
                .UseMongo(SetupMongo.ConnectionString, SetupMongo.Database)
                .ReplaceMongoHandler(handler)
                .Build();

            var @event = new TimerTriggeredEvent(run.Id);
            await fixture.Handle(@event);

            handler.Requests.Should().BeEquivalentTo(new[] {@event});
        }
    }
}