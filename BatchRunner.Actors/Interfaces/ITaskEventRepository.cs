using BatchRunner.Domain.Events;
using System;
using System.Collections.Generic;

namespace BatchRunner.Actors.Interfaces
{
    public interface ITaskEventRepository
    {
        /// <summary>
        /// Handles domain specific errors.
        /// </summary>
        public interface IErrorHandler
        {
            /// <summary>
            /// Called if the event to be inserted referenced a previous event that is
            /// no longer the latest event for the given task.
            /// </summary>
            /// <param name="outOfDateEvent"></param>
            /// <param name="latestEvent"></param>
            void PreviousEventNotUpToDate(ITaskEvent failedToAdd);
            void EventIdNotUnique(ITaskEvent failedToAdd, ITaskEvent preexistingEvent);
        }
        /// <summary>
        /// Tries to add the event to the log,
        /// provided the event was created in an up to date context
        /// (the event's PreviousEventId is the current head event's id)
        /// </summary>
        void AddTaskEvent(ITaskEvent taskEvent, IErrorHandler errorHandler);
        /// <summary>
        /// Returns all events for all tasks
        /// </summary>
        IEnumerable<ITaskEvent> GetAllTaskEvents();
        /// <summary>
        /// Returns all events for a given taskId
        /// </summary>
        IEnumerable<ITaskEvent> GetAllEventsFor(Guid taskId);
        /// <summary>
        /// Returns all events after the event with startId
        /// </summary>
        /// <param name="startId">The event with this Id will NOT be returned</param>
        IEnumerable<ITaskEvent> GetTaskEventsFrom(Guid startId);
        /// <summary>
        /// Returns all events for a given taskId, after the event with startId
        /// </summary>
        /// <param name="startId">The event with this Id will NOT be returned</param>
        IEnumerable<ITaskEvent> GetEventsForFrom(Guid taskId, Guid startId);
    }
}
