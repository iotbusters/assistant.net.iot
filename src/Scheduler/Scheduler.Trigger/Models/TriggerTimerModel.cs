using System;

namespace Assistant.Net.Scheduler.Trigger.Models;

/// <summary>
///     Trigger timer configuration model.
/// </summary>
/// <param name="RunId">Associated run ID.</param>
/// <param name="Arranged">The date when the run was arranged to be triggered.</param>
/// <param name="IsTriggered">Whether the timer was actually triggered.</param>
public record TriggerTimerModel(Guid RunId, DateTimeOffset Arranged, bool IsTriggered);
