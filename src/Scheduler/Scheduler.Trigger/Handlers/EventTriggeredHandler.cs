using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Commands;
using Assistant.Net.Scheduler.Contracts.Queries;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Trigger.Handlers;

internal sealed class EventTriggeredHandler : IAbstractHandler
{
    private readonly ILogger<EventTriggeredHandler> logger;
    private readonly INamedOptions<EventTriggerOptions> options;
    private readonly ITypeEncoder typeEncoder;
    private readonly IMessagingClient client;

    public EventTriggeredHandler(
        ILogger<EventTriggeredHandler> logger,
        INamedOptions<EventTriggerOptions> options,
        ITypeEncoder typeEncoder,
        IMessagingClient client)
    {
        this.logger = logger;
        this.options = options;
        this.typeEncoder = typeEncoder;
        this.client = client;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        await Publish(message, token);
        return Nothing.Instance;
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token)
    {
        var messageType = message.GetType();
        var messageName = typeEncoder.Encode(message.GetType())!;

        logger.LogDebug("Trigger({MessageType}) checking: begins.", messageName);

        if (!options.Value.EventTriggers.TryGetValue(messageType, out var runIds))
        {
            logger.LogError("Trigger({MessageType}) checking: not registered.", messageName);
            return;
        }

        logger.LogDebug("Trigger({MessageType}) checking: ends.", messageName);

        foreach (var runId in runIds)
        {
            var trigger = await client.Request(new TriggerQuery(runId), token);
            if (!trigger.IsActive)
            {
                logger.LogWarning("Trigger({MessageType}, {RunId}): inactive.", messageName, runId);
                continue;
            }

            logger.LogDebug("Trigger({MessageType}, {RunId}) processing: begins.", messageName, runId);

            if (!trigger.TriggerEventMask.All(mask => IsMatched(runId, message, mask.Key, mask.Value)))
            {
                logger.LogDebug("Trigger({MessageType}, {RunId}) processing: not matched.", messageName, runId);
                continue;
            }

            logger.LogDebug("Trigger({MessageType}, {RunId}) processing: matched.", messageName, runId);

            await client.Request(new RunSucceedCommand(runId), token);

            logger.LogInformation("Trigger({MessageType}, {RunId}) processing: ends.", messageName, runId);
            return;
        }
    }

    private bool IsMatched(Guid runId, object message, string maskPropertyName, string maskPropertyValue)
    {
        var messageType = message.GetType();
        var messageName = typeEncoder.Encode(messageType);

        var property = messageType.GetProperty(maskPropertyName);
        if (property?.GetMethod == null)
        {
            logger.LogCritical("Trigger({MessageType}, {RunId}) matching: no event get-property {MaskPropertyName} was found.",
                messageName, runId, maskPropertyName);
            return false;
        }

        var propertyValue = property.GetMethod.Invoke(message, Array.Empty<object?>())!;
        var propertyTypeName = typeEncoder.Encode(property.PropertyType)!;
        if (Match(property.PropertyType, propertyValue, maskPropertyValue) is not Some<bool>(var isMatched))
        {
            logger.LogCritical("Trigger({MessageType}, {RunId}) matching: {PropertyName}({PropertyType}) was unsupported.",
                messageName, runId, property.Name, propertyTypeName);
            return false;
        }

        if (!isMatched)
            logger.LogDebug(
                "Trigger({MessageType}, {RunId}) matching: "
                + "{PropertyName}(of {PropertyType}) unequal to {MaskPropertyName}({MaskPropertyValue}).",
                messageName, runId, property.Name, propertyTypeName, maskPropertyName, maskPropertyValue);

        return isMatched;
    }

    private static Option<bool> Match(Type propertyType, object propertyValue, string maskPropertyValue)
    {
        if (propertyType == typeof(string))
            return Option.Some(maskPropertyValue.Equals(propertyValue));

        if (propertyType == typeof(Guid))
            return Option.Some(Guid.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(DateTime))
            return Option.Some(DateTime.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(DateTimeOffset))
            return Option.Some(DateTimeOffset.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(bool))
            return Option.Some(bool.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(int))
            return Option.Some(int.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(long))
            return Option.Some(long.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(float))
            return Option.Some(float.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType == typeof(double))
            return Option.Some(double.TryParse(maskPropertyValue, out var value) && propertyValue.Equals(value));

        if (propertyType.IsEnum)
            return Option.Some(Enum.TryParse(propertyType, maskPropertyValue, out var value) && propertyValue.Equals(value));

        return Option.None;
    }
}
