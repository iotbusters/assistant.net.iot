using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Api.Models;

/// <summary>
///     Automation job action request create model.
/// </summary>
public sealed class JobActionCreateModel : JobCreateModel
{
    /// <summary>
    ///     Action message to request.
    /// </summary>
    public IMessage Action { get; set; } = null!;
}
