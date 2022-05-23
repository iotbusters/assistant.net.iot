using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers;

internal class GenericMessageHandler<TMessage> : IMessageHandler<TMessage>, IAbstractHandler where TMessage : IMessage
{
    private readonly Guid runId;
    private readonly ILogger<GenericMessageHandler<TMessage>> logger;
    private readonly IMessagingClient client;

    public GenericMessageHandler(
        Guid runId,
        ILogger<GenericMessageHandler<TMessage>> logger,
        IMessagingClient client)
    {
        this.runId = runId;
        this.logger = logger;
        this.client = client;
    }

    public async Task Handle(TMessage @event, CancellationToken token)
    {
        var eventTypeName = @event.GetType().Name;
        logger.LogInformation("{EventType} trigger: received.", eventTypeName);

        var trigger = await client.Request(new TriggerQuery(runId), token);

        var isMatched = trigger.TriggerEventMask.All(mask => IsMatched(@event, mask.Key, mask.Value));
        if (!isMatched)
        {
            logger.LogInformation("{EventType} trigger: skipped.", eventTypeName);
            return;
        }

        var succeeded = new RunStatusDto(RunStatus.Succeeded, "The event has been raised.");
        await client.Request(new RunUpdateCommand(runId, succeeded), token);
    }

    public async Task<object> Request(object message, CancellationToken token)
    {
        await Handle((TMessage)message, token);
        return null!;
    }

    public async Task Publish(object message, CancellationToken token) =>
        // note: it gives a 1ms window to fail the request.
        await await Task.WhenAny(
            Handle((TMessage)message, token),
            Task.Delay(TimeSpan.FromSeconds(0.001), token));

    private bool IsMatched(TMessage @event, string maskPropertyName, string maskPropertyValue)
    {
        var eventType = @event.GetType();
        var eventTypeName = eventType.Name;

        var property = eventType.GetProperty(maskPropertyName);
        if (property?.GetMethod == null)
        {
            logger.LogCritical("{EventType} trigger matching: no event get-property {MaskPropertyName} was found.",
                eventTypeName, maskPropertyName);
            return false;
        }

        var propertyValue = property.GetMethod.Invoke(@event, Array.Empty<object?>())!;
        if (Match(property.PropertyType, propertyValue, maskPropertyValue) is not Some<bool>(var isMatched))
        {
            logger.LogCritical("{EventType} trigger matching: {PropertyName}({PropertyType}) was unsupported.",
                eventTypeName, property.Name, property.PropertyType.Name);
            return false;
        }

        if (!isMatched)
        {
            logger.LogInformation(
                "{EventType} trigger matching: {PropertyName}({PropertyType}) failed on {MaskPropertyName}:{MaskPropertyValue}.",
                eventTypeName, property.Name, property.PropertyType.Name, maskPropertyName, maskPropertyValue);
            return false;
        }

        return true;
    }

    private static Option<bool> Match(Type propertyType, object propertyValue, string maskPropertyValue)
    {
        if (propertyType == typeof(string))
            return Option.Some(!maskPropertyValue.Equals(propertyValue));

        if (propertyType == typeof(string))
            return Option.Some(!maskPropertyValue.Equals(propertyValue));

        if (propertyType == typeof(bool))
            return Option.Some(!bool.TryParse(maskPropertyValue, out var boolValue)
                               || !propertyValue.Equals(boolValue));

        if (propertyType == typeof(int))
            return Option.Some(!int.TryParse(maskPropertyValue, out var intValue)
                               || !propertyValue.Equals(intValue));

        if (propertyType == typeof(long))
            return Option.Some(!long.TryParse(maskPropertyValue, out var longValue)
                               || !propertyValue.Equals(longValue));

        if (propertyType == typeof(float))
            return Option.Some(!float.TryParse(maskPropertyValue, out var floatValue)
                               || !propertyValue.Equals(floatValue));

        if (propertyType == typeof(double))
            return Option.Some(!double.TryParse(maskPropertyValue, out var doubleValue)
                               || !propertyValue.Equals(doubleValue));

        if (propertyType.IsEnum)
            return Option.Some(!Enum.TryParse(propertyType, maskPropertyValue, out var enumValue)
                               || !propertyValue.Equals(enumValue));

        return Option.None;
    }
}
