using BatchRunner.Domain.Commands;

namespace BatchRunner.Domain.CommandHandlers
{
    public interface ICommandHandler<T> where T : ICommand
    {
        void Handle(T command);
    }
}
