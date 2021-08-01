using System;

namespace BatchRunner.Domain.Commands
{
    public interface ICommand
    {
        Guid CmdId { get; }
        void Visit(ICommandVisitor visitor);
    }

}
