using Assistant.Net.Abstractions;
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

internal class TriggerEventHandler : IAbstractHandler
{
    private readonly Guid runId;
    private readonly ILogger<TriggerEventHandler> logger;
    private readonly IMessagingClient client;
    private readonly ITypeEncoder typeEncoder;

    public TriggerEventHandler(
        Guid runId,
        ILogger<TriggerEventHandler> logger,
        IMessagingClient client,
        ITypeEncoder typeEncoder)
    {
        this.runId = runId;
        this.logger = logger;
        this.client = client;
        this.typeEncoder = typeEncoder;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        await Publish(message, token);
        return Nothing.Instance;
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token)
    {
        var eventTypeName = typeEncoder.Encode(message.GetType());
        logger.LogDebug("Trigger({EventType}, {RunId}): received.", eventTypeName, runId);

        var trigger = await client.Request(new TriggerQuery(runId), token);
        if (!trigger.IsActive)
        {
            logger.LogWarning("Trigger({EventType}, {RunId}): inactive.", eventTypeName, runId);
            return;
        }

        if (!trigger.TriggerEventMask.All(mask => IsMatched(message, mask.Key, mask.Value)))
        {
            logger.LogDebug("Trigger({EventType}, {RunId}): not matched.", eventTypeName, runId);
            return;
        }

        logger.LogDebug("Trigger({EventType}, {RunId}): matched.", eventTypeName, runId);

        var succeeded = new RunStatusDto(RunStatus.Succeeded, "The event has been raised.");
        await client.Request(new RunUpdateCommand(runId, succeeded), token);

        logger.LogInformation("Trigger({EventType}, {RunId}): succeeded.", eventTypeName, runId);
    }

    private bool IsMatched(object @event, string maskPropertyName, string maskPropertyValue)
    {
        var eventType = @event.GetType();
        var eventTypeName = typeEncoder.Encode(eventType);

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
            logger.LogDebug(
                "{EventType} trigger matching: {PropertyName}({PropertyType}) unequal to {MaskPropertyName}({MaskPropertyValue}).",
                eventTypeName, property.Name, property.PropertyType.Name, maskPropertyName, maskPropertyValue);

        return isMatched;
    }

    private static Option<bool> Match(Type propertyType, object propertyValue, string maskPropertyValue)
    {
        if (propertyType == typeof(string))
            return Option.Some(!maskPropertyValue.Equals(propertyValue));

        if (propertyType == typeof(Guid))
            return Option.Some(!Guid.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(DateTime))
            return Option.Some(!DateTime.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(DateTimeOffset))
            return Option.Some(!DateTimeOffset.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(bool))
            return Option.Some(!bool.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(int))
            return Option.Some(!int.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(long))
            return Option.Some(!long.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(float))
            return Option.Some(!float.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType == typeof(double))
            return Option.Some(!double.TryParse(maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        if (propertyType.IsEnum)
            return Option.Some(!Enum.TryParse(propertyType, maskPropertyValue, out var value)
                               || !propertyValue.Equals(value));

        return Option.None;
    }
}
