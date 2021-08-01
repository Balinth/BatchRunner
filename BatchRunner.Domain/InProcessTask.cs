using System;

namespace BatchRunner.Domain
{
    /// <summary>
    /// These tasks can be used by a single tool
    /// for granual progress reporting.
    /// </summary>
    public record InProcessTask : ITask
    {
        /// <inheritdoc/>
        public Guid TaskId { get; init; }

        /// <inheritdoc/>
        public string Name { get; init; }

        /// <inheritdoc/>
        public string Description { get; init; }

        public InProcessTask(Guid taskId, string name, string description)
        {
            TaskId = taskId;
            Name = name;
            Description = description;
        }
    }
}
