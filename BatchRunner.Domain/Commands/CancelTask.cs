using System;

namespace BatchRunner.Domain.Commands
{
    public record CancelTask : ICommand
    {
        public Guid CmdId { get; init; }
        public ITask Task { get; init; }

        public CancelTask(Guid cmdId, ITask task)
        {
            CmdId = cmdId;
            Task = task;
        }

        public void Visit(ICommandVisitor visitor) => visitor.Visit(this);
    }
}
