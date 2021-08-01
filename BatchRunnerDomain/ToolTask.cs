using System;
using System.Collections.Immutable;

namespace BatchRunner.Domain
{
    // Note: this would be the ideal syntax to use
    // but due to swagger and related tools lagging behind on support
    // for doc comments on these parameters
    // instead the more verbose init property declaration
    // with manual constructor definition is used.
    //public record ToolTask(
    //    Guid TaskId,
    //    string ToolPath,
    //    string Name,
    //    string Description,
    //    ImmutableList<string> Arguments) : ITask
    //{
    //}

    /// <summary>
    /// Represents a task that has to be done by a CLI tool
    /// </summary>
    public record ToolTask : ITask
    {
        /// <inheritdoc/>
        public Guid TaskId { get; init; }

        /// <inheritdoc/>
        public string Name { get; init; }

        /// <inheritdoc/>
        public string Description { get; init; }

        /// <summary>
        /// File system path of the tool to be run.
        /// </summary>
        public string ToolPath { get; init; }

        /// <summary>
        /// CommandLine arguments of the tool describing the task
        /// </summary>
        public string Arguments { get; init; }

        public ToolTask(Guid taskId, string toolPath, string name, string description, string arguments)
        {
            TaskId = taskId;
            ToolPath = toolPath;
            Name = name;
            Description = description;
            Arguments = arguments;
        }
    }
}
