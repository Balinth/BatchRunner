using System;
using System.Collections.Generic;
using LiteDB;
using BatchRunner.Domain;
using BatchRunner.Domain.Interfaces;
using BatchRunner.Domain.Events;
using System.Linq;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;

namespace BatchRunner.Persistence.LiteDB
{
    public sealed class TaskEventRepository : ITaskEventRepository
    {
        private readonly ILiteDatabase liteDatabase;

        public class OrderAnnotatedEvent
        {
            public OrderAnnotatedEvent(int eventNumber, Guid id, IEvent @event)
            {
                EventNumber = eventNumber;
                _id = id;
                Event = @event;
            }
            public int EventNumber { get; set; }
            public Guid _id { get; set; }
            public IEvent Event { get; set; }
        }

        private class TaskEventDeserializer : ITaskEventSource<BsonDocument>
        {
            private readonly BsonMapper mapper;

            public static void AddAllDeserializers(BsonMapper mapper)
            {
                TaskEventDeserializer taskEventDeserializer = new(mapper);
                var addDeserializerInfo = typeof(TaskEventDeserializer).GetMethod(nameof(AddDeserializerFor), BindingFlags.Static | BindingFlags.NonPublic);
                foreach (var deserializerInfo in typeof(ITaskEventSource<BsonDocument>).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var concreteAdder = addDeserializerInfo!.MakeGenericMethod(deserializerInfo.ReturnType);
                    var concreteDeserializer = deserializerInfo.CreateDelegate(concreteAdder.GetParameters()[1].ParameterType, taskEventDeserializer);
                    concreteAdder.Invoke(null, new object[] { mapper, concreteDeserializer });
                    Thread.Sleep(1000);
                }
            }

            private static void AddDeserializerFor<T>(BsonMapper mapper, Func<BsonDocument,T> deserializer)
            {
                mapper.Entity<T>()
                    .Ctor(deserializer);
            }

            public TaskEventDeserializer(BsonMapper mapper)
            {
                this.mapper = mapper;
            }

            public TaskCancelled CreateTaskCancelled(BsonDocument source)
            {
                return new TaskCancelled(
                    eventId: source[nameof(TaskCancelled.EventId)],
                    task: mapper.Deserialize<ITask>(source[nameof(TaskCancelled.Task)]),
                    previousEventId: source[nameof(TaskCancelled.PreviousEventId)]
                    );
            }

            public TaskFailed CreateTaskFailed(BsonDocument source)
            {
                return new TaskFailed(
                    eventId: source[nameof(TaskCancelled.EventId)],
                    task: mapper.Deserialize<ITask>(source[nameof(TaskCancelled.Task)]),
                    previousEventId: source[nameof(TaskCancelled.PreviousEventId)]
                    );
            }

            public TaskFinished CreateTaskFinished(BsonDocument source)
            {
                return new TaskFinished(
                    eventId: source[nameof(TaskCancelled.EventId)],
                    task: mapper.Deserialize<ITask>(source[nameof(TaskCancelled.Task)]),
                    previousEventId: source[nameof(TaskCancelled.PreviousEventId)]
                    );
            }

            public TaskScheduled CreateTaskScheduled(BsonDocument source)
            {
                return new TaskScheduled(
                    eventId: source[nameof(TaskScheduled.EventId)],
                    task: mapper.Deserialize<ITask>(source[nameof(TaskScheduled.Task)]),
                    previousEventId: source[nameof(TaskScheduled.PreviousEventId)],
                    priority: source[nameof(TaskScheduled.Priority)]
                    );
            }

            public TaskStarted CreateTaskStarted(BsonDocument source)
            {
                var subTasks =
                    source[nameof(TaskStarted.SubTasks)]
                    .AsArray
                    .Select(doc => mapper.Deserialize<ITask>(doc))
                    .ToImmutableList();
                return new TaskStarted(
                    eventId: source[nameof(TaskStarted.EventId)],
                    task: mapper.Deserialize<ITask>(source[nameof(TaskStarted.Task)]),
                    previousEventId: source[nameof(TaskStarted.PreviousEventId)],
                    subTasks: subTasks
                    );
            }
        }

        //private ILiteCollection<IEvent> EventCollection => liteDatabase.GetCollection<IEvent>("events");
        private ILiteCollection<OrderAnnotatedEvent> EventOrderCollection => liteDatabase.GetCollection<OrderAnnotatedEvent>("events");

        /// <summary>
        /// Note: Should only ever construct a single repository for a single database file
        /// due to performance / file access considerations. Opens the database in Direct mode.
        /// The created repository can be left open and reused as many times as needed.
        /// </summary>
        /// <param name="liteDatabaseFilePath"></param>
        public TaskEventRepository(ILiteDatabase liteDatabase, bool usingReflection = true)
        {
            BsonMapper mapper = liteDatabase.Mapper;
            mapper.Entity<OrderAnnotatedEvent>()
                .Id(o => o._id, false)
                .Ctor(doc => new OrderAnnotatedEvent(
                    eventNumber: doc[nameof(OrderAnnotatedEvent.EventNumber)],
                    id: doc["_id"],
                    @event: mapper.Deserialize<IEvent>(doc[nameof(OrderAnnotatedEvent.Event)])));

            if (usingReflection)
            {
                TaskEventDeserializer.AddAllDeserializers(mapper);
            }
            else
            {
                var eventDeserializer = new TaskEventDeserializer(mapper);

                mapper.Entity<TaskScheduled>()
                    .Ctor(doc => eventDeserializer.CreateTaskScheduled(doc));
                mapper.Entity<TaskCancelled>()
                    .Ctor(doc => eventDeserializer.CreateTaskCancelled(doc));
                mapper.Entity<TaskStarted>()
                    .Ctor(doc => eventDeserializer.CreateTaskStarted(doc));
                mapper.Entity<TaskFinished>()
                    .Ctor(doc => eventDeserializer.CreateTaskFinished(doc));
                mapper.Entity<TaskFailed>()
                    .Ctor(doc => eventDeserializer.CreateTaskFailed(doc));
            }

            this.liteDatabase = liteDatabase;
            EventOrderCollection.EnsureIndex(o => o.EventNumber);
        }

        public void AddTaskEvent(ITaskEvent taskEvent, ITaskEventRepository.IErrorHandler errorHandler)
        {
            if (EventOrderCollection.FindById(taskEvent.EventId) is OrderAnnotatedEvent previousEvent)
            {
                errorHandler.EventIdNotUnique(taskEvent, previousEvent.Event);
                return;
            }

            int eventNumber = 0;

            if (
                EventOrderCollection
                .Find(Query.All(nameof(OrderAnnotatedEvent.EventNumber),Query.Descending))
                //.OrderByDescending(o => o.EventNumber)
                .FirstOrDefault() is OrderAnnotatedEvent lastEvent)
            {
                if(lastEvent.Event.EventId != taskEvent.PreviousEventId)
                {
                    errorHandler.PreviousEventNotUpToDate(taskEvent, lastEvent.Event.EventId);
                    return;
                }
                eventNumber = lastEvent.EventNumber + 1;
            }
            EventOrderCollection.Insert(new OrderAnnotatedEvent(eventNumber, taskEvent.EventId, taskEvent));
        }

        public IEnumerable<ITaskEvent> GetAllTaskEvents()
        {
            return EventOrderCollection
                .Find(Query.All(nameof(OrderAnnotatedEvent.EventNumber), Query.Descending))
                //.OrderByDescending(o => o.EventNumber)
                //.FindAll()
                //.OrderBy(o => o.EventNumber)
                .Select(o => o.Event)
                .OfType<ITaskEvent>();
        }

        public IEnumerable<ITaskEvent> GetAllTaskEventsFor(Guid taskId)
        {
            return GetAllTaskEvents()
                .Where(e => e.Task.TaskId == taskId);
        }

        public IEnumerable<ITaskEvent> GetTaskEventsFrom(Guid startId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITaskEvent> GetTaskEventsForFrom(Guid taskId, Guid startId)
        {
            throw new NotImplementedException();
        }
    }
}
