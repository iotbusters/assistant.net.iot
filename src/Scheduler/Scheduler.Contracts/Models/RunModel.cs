using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation run model.
/// </summary>
public sealed class RunModel
{
    /// <summary/>
    private RunModel(Guid id, Guid? nextRunId, Guid automationId, JobModel jobSnapshot, RunStatus status)
    {
        Id = id;
        NextRunId = nextRunId;
        AutomationId = automationId;
        JobSnapshot = jobSnapshot;
        Status = status;
    }

    /// <summary>
    ///     Creates scheduled run model.
    /// </summary>
    public RunModel(Guid id, Guid? nextRunId, Guid automationId, JobModel jobSnapshot)
    {
        Id = id;
        NextRunId = nextRunId;
        AutomationId = automationId;
        JobSnapshot = jobSnapshot;
        Status = RunStatus.Scheduled;
    }

    /// <summary>
    ///     Run id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Parent run id.
    /// </summary>
    public Guid? NextRunId { get; }

    /// <summary>
    ///     Related automation id.
    /// </summary>
    public Guid AutomationId { get; }

    /// <summary>
    ///     Related job snapshot.
    /// </summary>
    [Required]
    public JobModel JobSnapshot { get; }

    /// <summary>
    ///     Latest status of the run.
    /// </summary>
    public RunStatus Status { get; }

    /// <summary>
    ///     Creates a copy of <see cref="RunModel"/> with new <paramref name="status"/>.
    /// </summary>
    public RunModel WithStatus(RunStatus status)
    {
        switch (Status, status)
        {
            case (RunStatus.Scheduled, RunStatus.Started):
                break;
            case (RunStatus.Scheduled, RunStatus.Succeeded):
                break;
            case (RunStatus.Scheduled, RunStatus.Failed):
                break;
            case (RunStatus.Started, RunStatus.Succeeded):
                break;
            case (RunStatus.Started, RunStatus.Failed):
                break;
            case var (a, b) when a == b:
                return this;

            default:
                throw new InvalidRequestException($"Run({Id}) cannot change the status from {Status} to {status}.");
        }

        return new(Id, NextRunId, AutomationId, JobSnapshot, status);
    }
}
