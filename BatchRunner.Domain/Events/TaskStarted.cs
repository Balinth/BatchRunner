using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.Events
{
    public record TaskStarted : ITaskEvent
    {
        public Guid EventId { get; init; }
        public ITask Task { get; init; }
        public Guid PreviousEventId { get; init; }
        /// <summary>
        /// Projected subtasks that need to be finished for this task to be finished.
        /// Not binding, a task can be validly finished even if not all of these are finished.
        /// Mainly used for progress reporting.
        /// </summary>
        public ImmutableList<ITask> SubTasks { get; init; }


        public TaskStarted(Guid eventId, ITask task, Guid previousEventId, ImmutableList<ITask> subTasks)
        {
            EventId = eventId;
            Task = task;
            SubTasks = subTasks;
            PreviousEventId = previousEventId;
        }

        public void Visit(ITaskEventVisitor visitor) => visitor.Visit(this);

        public virtual bool Equals(TaskStarted? other)
        {
            if (other is null)
                return false;
            if (SubTasks.Count != other.SubTasks.Count)
                return false;
            bool result =
                SubTasks
                .Zip(other.SubTasks)
                .Select(t => t.First == t.Second )
                .All(t => t);
            return EventId == other.EventId
                && Task.Equals(other.Task)
                && PreviousEventId == other.PreviousEventId
                && result;
        }
        public override int GetHashCode()
        {
            int hc = EventId.GetHashCode();
            hc ^= Task.GetHashCode();
            hc ^= PreviousEventId.GetHashCode();
            foreach (var subTask in SubTasks)
                hc ^= subTask.GetHashCode();
            return hc;
        }
    }
}
