namespace BatchRunner.Domain.Events
{
    public interface ITaskEvent : IEvent
    {
        /// <summary>
        /// The task that this event relates to
        /// </summary>
        ITask Task { get; }
        void Visit(ITaskEventVisitor visitor);
        void IEvent.Visit(IEventVisitor visitor) => visitor.Visit(this);
    }
}
