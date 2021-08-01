using System;

namespace BatchRunner.Domain.Events
{
    public record TaskCancelled : ITaskEvent
    {
        public Guid EventId { get; init; }
        public ITask Task { get; init; }
        public Guid PreviousEventId { get; init; }

        public TaskCancelled(Guid eventId, ITask task, Guid previousEventId)
        {
            EventId = eventId;
            Task = task;
            PreviousEventId = previousEventId;
        }

        public void Visit(ITaskEventVisitor visitor) => visitor.Visit(this);
    }
}
