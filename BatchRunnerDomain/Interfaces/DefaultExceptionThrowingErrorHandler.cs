using BatchRunner.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.Interfaces
{
    /// <summary>
    /// Simple exception throwing default error handler.
    /// Thrown exceptions are ApplicationExceptions with custom human readable message
    /// and Data containing the reported error parameters.
    /// </summary>
    public class DefaultExceptionThrowingErrorHandler : ITaskEventRepository.IErrorHandler, ITaskRepository.IErrorHandler
    {
        private DefaultExceptionThrowingErrorHandler() { }
        private static DefaultExceptionThrowingErrorHandler instance = new DefaultExceptionThrowingErrorHandler();
        public static DefaultExceptionThrowingErrorHandler Instance => instance;
        void ITaskEventRepository.IErrorHandler.EventIdNotUnique(ITaskEvent failedToAdd, IEvent preexistingEvent)
        {
            throw new EventIdNotUnique(
                failedToAdd,
                preexistingEvent,
                $"Failed to add task event {{{failedToAdd.EventId}}}. An event with this ID already exists in the log.");
        }

        void ITaskEventRepository.IErrorHandler.PreviousEventNotUpToDate(ITaskEvent failedToAdd, Guid currentLastEventID)
        {
            throw new PreviousEventNotUpToDate(
                currentLastEventID,
                failedToAdd,
                $"Failed to add task event {{{failedToAdd.EventId}}}. Its previousEvent is no longer the latest in the event log. Current latest event id: {currentLastEventID}");
        }

        void ITaskRepository.IErrorHandler.TaskIdNotUnique(ITask failedToAdd, ITask preexistingTask)
        {
            throw new TaskIdNotUnique(
                failedToAdd,
                preexistingTask,
                $"Failed to add task {{{failedToAdd.ToString()}}}. Its id is already taken by {preexistingTask}.");
        }
        public class PreviousEventNotUpToDate : ApplicationException
        {
            public PreviousEventNotUpToDate(Guid currentLastEventID, ITaskEvent failedToAdd, string message) : base(message)
            {
                CurrentLastEventID = currentLastEventID;
                FailedToAdd = failedToAdd;
            }

            public Guid CurrentLastEventID { get; }
            public ITaskEvent FailedToAdd { get; }
        }
        public class TaskIdNotUnique : ApplicationException
        {
            public TaskIdNotUnique(ITask failedToAdd, ITask preexistingTask, string message) : base(message)
            {
                FailedToAdd = failedToAdd;
                PreexistingTask = preexistingTask;
            }

            public ITask FailedToAdd { get; }
            public ITask PreexistingTask { get; }
        }
        public class EventIdNotUnique : ApplicationException
        {
            public EventIdNotUnique(ITaskEvent failedToAdd, IEvent preexistingEvent, string message) : base(message)
            {
                FailedToAdd = failedToAdd;
                PreexistingEvent = preexistingEvent;
            }

            public ITaskEvent FailedToAdd { get; }
            public IEvent PreexistingEvent { get; set; }
        }
    }
}
