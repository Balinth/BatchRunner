using BatchRunner.Domain.Aggregates;
using BatchRunner.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Domain.Projections
{
    public static class TasksReadyToStart
    {
        public static IEnumerable<ITask> ProjectInPriorityOrder(ITaskRepository taskRepository, ITaskEventRepository taskEventRepository)
        {
            return taskRepository.GetAllTasks()
                .Select(task => new TaskState(task, taskEventRepository))
                .Where(task => task.State == TaskState.ExecutionState.Scheduled)
                .OrderByDescending(task => task.Priority)
                .Select(task => task.Task);
        }
    }
}
