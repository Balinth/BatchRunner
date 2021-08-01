namespace BatchRunner.Domain.Commands
{
    public interface ICommandVisitor
    {
        void Visit(ScheduleTask target);
        void Visit(CancelTask target);
    }

}
