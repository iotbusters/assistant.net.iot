using Assistant.Net.Scheduler.Contracts.Enums;
using Assistant.Net.Scheduler.Contracts.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation run model.
/// </summary>
public class RunModel
{
    /// <summary/>
    private RunModel(Guid id, Guid? nextRunId, Guid automationId, JobModel jobSnapshot, RunStatusDto status)
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
        Status = new RunStatusDto(RunStatus.Scheduled);
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
    [Required]
    public RunStatusDto Status { get; }

    /// <summary>
    ///     Creates a copy of <see cref="RunModel"/> with new <paramref name="status"/>.
    /// </summary>
    public RunModel WithStatus(RunStatusDto status)
    {
        switch (Status.Value, status.Value)
        {
            case (RunStatus.Scheduled, RunStatus.Started):
                break;

            case (RunStatus.Started, RunStatus.Succeeded):
                break;

            case (RunStatus.Started, RunStatus.Failed):
                break;

            case var (a, b) when a == b:
                return this;

            default:
                throw new InvalidRequestException($"Run({Id}) cannot change the status from {Status.Value} to {status.Value}.");
        }

        return new(Id, NextRunId, AutomationId, JobSnapshot, status);
    }
}
