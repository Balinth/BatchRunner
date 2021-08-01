using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.Events
{
    public record TaskScheduled : ITaskEvent
    {
        public TaskScheduled(ITask task, Guid eventId, Guid previousEventId, int priority)
        {
            Task = task;
            EventId = eventId;
            PreviousEventId = previousEventId;
            Priority = priority;
        }

        public ITask Task { get; init; }

        public Guid EventId { get; init; }

        public Guid PreviousEventId { get; init; }

        /// <summary>
        /// negative infinity is lowest,
        /// positive infinity is highest priority.
        /// </summary>
        public int Priority { get; init; }

        public void Visit(ITaskEventVisitor visitor) => visitor.Visit(this);
    }
}
