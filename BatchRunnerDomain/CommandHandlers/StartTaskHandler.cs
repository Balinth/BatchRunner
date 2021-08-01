using BatchRunner.Domain.Aggregates;
using BatchRunner.Domain.Commands;
using BatchRunner.Domain.Events;
using BatchRunner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.CommandHandlers
{
    public class StartTaskHandler : ICommandHandler<ScheduleTask>
    {
        private readonly ITaskRepository taskRepository;
        private readonly ITaskEventRepository taskEventRepository;

        public StartTaskHandler(ITaskRepository taskRepository, ITaskEventRepository taskEventRepository)
        {
            this.taskRepository = taskRepository;
            this.taskEventRepository = taskEventRepository;
        }

        public Action TaskAlreadyStarted { get; set; } = () => throw new ApplicationException();

        public void Handle(ScheduleTask command)
        {
            var taskState = new TaskState(command.Task, taskEventRepository);
            if (taskState.CanBeScheduled == false)
                TaskAlreadyStarted();
            else
            {
                taskEventRepository.AddTaskEvent(null, DefaultExceptionThrowingErrorHandler.Instance);
            }
        }

        private class ErrorHandler : ITaskEventRepository.IErrorHandler
        {
            public void EventIdNotUnique(ITaskEvent failedToAdd, IEvent preexistingEvent)
            {
                throw new NotImplementedException();
            }
            public void PreviousEventNotUpToDate(ITaskEvent failedToAdd, Guid currentLatestEventID)
            {
                throw new NotImplementedException();
            }
        }
    }
}
