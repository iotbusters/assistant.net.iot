using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Trigger.Options;

public class TriggerPollingOptions
{
    [Required]
    public TimeSpan PollingWait { get; set; } = TimeSpan.FromMinutes(5);

    [Required]
    public TimeSpan PollingTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
