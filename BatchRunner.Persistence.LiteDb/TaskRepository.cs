using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using LiteDB;
using BatchRunner.Domain;
using BatchRunner.Domain.Interfaces;
using System.Linq;

namespace BatchRunner.Persistence.LiteDB
{
    public sealed class TaskRepository : ITaskRepository
    {
        private readonly ILiteDatabase liteDatabase;

        private ILiteCollection<ITask> TaskCollection => liteDatabase.GetCollection<ITask>("tasks");

        /// <summary>
        /// Note: Should only ever construct a single repository for a single database file
        /// due to performance / file access considerations. Opens the database in Direct mode.
        /// The created repository can be left open and reused as many times as needed.
        /// </summary>
        /// <param name="liteDatabaseFilePath"></param>
        public TaskRepository(ILiteDatabase liteDatabase)
        {
            Func<BsonDocument, ToolTask> toolTaskCtor = doc => new ToolTask(
                     taskId: doc["_id"],
                     toolPath: doc[nameof(ToolTask.ToolPath)],
                     name: doc[nameof(ToolTask.Name)],
                     description: doc[nameof(ToolTask.Description)],
                     arguments: doc[nameof(ToolTask.Arguments)]);

            Func<BsonDocument, InProcessTask> inProcessTaskCtor = doc => new InProcessTask(
                     taskId: doc["_id"],
                     name: doc[nameof(ToolTask.Name)],
                     description: doc[nameof(ToolTask.Description)]);

            BsonMapper mapper = liteDatabase.Mapper;
            mapper.Entity<ToolTask>()
                .Id(task => task.TaskId, false)
                .Ctor(toolTaskCtor);
            mapper.Entity<InProcessTask>()
                .Id(task => task.TaskId, false)
                .Ctor(inProcessTaskCtor);
            mapper.Entity<ITask>()
                .Id(task => task.TaskId, false);
            this.liteDatabase = liteDatabase;
        }

        public void AddTask(ITask task, ITaskRepository.IErrorHandler errorHandler)
        {
            if (TaskCollection.FindById(task.TaskId) is ITask preexistingTask)
            {
                errorHandler.TaskIdNotUnique(task, preexistingTask);
                return;
            }
            TaskCollection.Insert(task);
        }

        public IEnumerable<ITask> GetAllTasks()
        {
            return TaskCollection.FindAll();
        }

        public ITask? GetTask(Guid id)
        {
            return TaskCollection.FindById(id);
        }
    }
}
