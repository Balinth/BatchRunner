namespace BatchRunner.Domain.Events
{
    public interface ITaskEventVisitor
    {
        void Visit(TaskScheduled target);
        void Visit(TaskStarted target);
        void Visit(TaskFailed target);
        void Visit(TaskCancelled target);
        void Visit(TaskFinished target);
    }
}
