namespace BatchRunner.Domain.Events
{
    public interface IEventVisitor
    {
        void Visit(ITaskEvent target);
    }
}
