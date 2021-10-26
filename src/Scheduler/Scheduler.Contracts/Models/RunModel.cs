using Assistant.Net.Scheduler.Contracts.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models
{
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

        /// <summary/>
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
        public RunModel WithStatus(RunStatusDto status) => new(Id, NextRunId, AutomationId, JobSnapshot, status);
    }
}
