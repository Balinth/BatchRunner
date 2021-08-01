using System;

namespace BatchRunner.Domain
{
    /// <summary>
    /// Describes a task.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Unique Id of the task.
        /// Note: should be also unique
        /// between reruns of the same task.
        /// </summary>
        Guid TaskId { get; }

        /// <summary>
        /// Non-unique descriptive name of the task
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Humanreadable description of the task
        /// </summary>
        string Description { get; }
    }
}
