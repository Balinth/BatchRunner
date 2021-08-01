using BatchRunner.Domain;
using System;
using System.Collections.Generic;

namespace BatchRunner.Actors.Interfaces
{
    public interface ITaskRepository
    {
        public interface IErrorHandler
        {
            void TaskIdNotUnique(ITask failedToAdd, ITask preexistingTask);
        }
        void AddTask(ITask task, IErrorHandler errorHandler);
        IEnumerable<ITask> GetAllTasks();
        ITask GetTask(Guid id);
    }
}
