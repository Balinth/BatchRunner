using BatchRunner.Domain.Events;
using BatchRunner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.Aggregates
{
    public class TaskState : ITaskEventVisitor
    {
        private readonly ITaskEventRepository taskEventRepository;

        public enum ExecutionState
        {
            /// <summary>
            /// The task is ready to be started by someone willing
            /// </summary>
            Scheduled,
            Started,
            Failed,
            Cancelled,
            Finished,
            /// <summary>
            /// The task is not ready to be started yet.
            /// Note: possibly it is not ever intended to be started by someone else
            /// in case it is just used for progress reporting.
            /// </summary>
            Pending,
        }

        public ExecutionState State { get; private set; } = ExecutionState.Pending;
        public int Priority { get; private set; } = 0;
        public ITask Task { get; private set; }

        public TaskState(ITask task, ITaskEventRepository taskEventRepository)
        {
            Task = task;
            this.taskEventRepository = taskEventRepository;

            foreach (var taskEvent in taskEventRepository.GetAllTaskEventsFor(task.TaskId))
                taskEvent.Visit(this);
        }

        public bool CanBeScheduled => State == ExecutionState.Pending || State == ExecutionState.Scheduled;

        public void Visit(TaskScheduled target)
        {
            State = ExecutionState.Scheduled;
            Priority = target.Priority;
        }

        public void Visit(TaskStarted target) => State = ExecutionState.Started;

        public void Visit(TaskFailed target) => State = ExecutionState.Failed;

        public void Visit(TaskFinished target) => State = ExecutionState.Finished;

        public void Visit(TaskCancelled target) => State = ExecutionState.Cancelled;

    }
}
