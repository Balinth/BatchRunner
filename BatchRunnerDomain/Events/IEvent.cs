using System;

namespace BatchRunner.Domain.Events
{
    /// <summary>
    /// Describes an event in the system.
    /// All state changes should be described by an event
    /// that implements this, or a more derived interface.
    /// </summary>
    public interface IEvent
    {
        Guid EventId { get; }
        Guid PreviousEventId { get; }
        void Visit(IEventVisitor visitor);
    }
}
