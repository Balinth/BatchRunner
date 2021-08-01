using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BatchRunner.Domain.Commands
{
    public record ScheduleTask : ICommand
    {
        public Guid CmdId { get; init; }
        public ITask Task { get; init; }
        public string ToolPath { get; init; }
        public ImmutableList<string> Arguments { get; init; }

        public ScheduleTask(Guid cmdId, ITask task, string toolPath, ImmutableList<string> arguments)
        {
            CmdId = cmdId;
            Task = task;
            ToolPath = toolPath;
            Arguments = arguments;
        }

        public void Visit(ICommandVisitor visitor) => visitor.Visit(this);
    }
}
