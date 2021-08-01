using System;

namespace BatchRunner.Domain.Events
{
    public record TaskFailed : ITaskEvent
    {
        public Guid EventId { get; init; }
        public ITask Task { get; init; }
        public Guid PreviousEventId { get; init; }

        public TaskFailed(Guid eventId, ITask task, Guid previousEventId)
        {
            EventId = eventId;
            Task = task;
            PreviousEventId = previousEventId;
        }

        public void Visit(ITaskEventVisitor visitor) => visitor.Visit(this);
    }
}
