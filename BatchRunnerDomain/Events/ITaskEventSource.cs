namespace BatchRunner.Domain.Events
{
    public interface ITaskEventSource<T>
    {
        TaskScheduled CreateTaskScheduled(T source);
        TaskStarted CreateTaskStarted(T source);
        TaskFailed CreateTaskFailed(T source);
        TaskCancelled CreateTaskCancelled(T source);
        TaskFinished CreateTaskFinished(T source);
    }
}
