using BatchRunner.Domain;
using BatchRunner.Domain.Events;
using BatchRunner.Domain.Interfaces;
using BatchRunner.Persistence.LiteDB;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;

namespace BatchRunner.Persistence.Test
{
    public class LiteDBTests
    {
        [Fact]
        public void LiteDBCanStoreToolTasks()
        {
            // Arrange
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskRepository(db);
                var toolTaskId = Guid.NewGuid();
                ITask expectedToolTask = new ToolTask(toolTaskId, "someTool", "Test task", "This is a task used to test the tasks' persistency solution", "--someArg");

                // Act
                sut.AddTask(expectedToolTask, DefaultExceptionThrowingErrorHandler.Instance);
                var actualToolTask = sut.GetTask(toolTaskId);

                // Assert
                Assert.Equal(expectedToolTask, actualToolTask);
            }
        }

        [Fact]
        public void LiteDBCanStoreInProcessTasks()
        {
            // Arrange
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskRepository(db);
                var inProcessTaskId = Guid.NewGuid();
                ITask expectedInProcessTask = new InProcessTask(inProcessTaskId, "Init", "Init system.");

                // Act
                sut.AddTask(expectedInProcessTask, DefaultExceptionThrowingErrorHandler.Instance);
                var actualInProcessTask = sut.GetTask(inProcessTaskId);

                // Assert
                Assert.Equal(expectedInProcessTask, actualInProcessTask);
            }
        }

        public static IEnumerable<object[]> EventStream
        {
            get
            {
                ITask task = new ToolTask(Guid.NewGuid(), "someTool", "Test task", "This is a task used to test the tasks' persistency solution", "--someArg");
                yield return new object[] { new TaskScheduled(task, Guid.NewGuid(), Guid.Empty, 1) };
                yield return new object[] { new TaskCancelled(Guid.NewGuid(), task, Guid.Empty) };
                yield return new object[] { new TaskStarted(Guid.NewGuid(), task, Guid.Empty, Enumerable.Empty<ITask>().ToImmutableList()) };
                yield return new object[] { new TaskFailed(Guid.NewGuid(), task, Guid.Empty) };
                yield return new object[] { new TaskFinished(Guid.NewGuid(), task, Guid.Empty) };
            }
        }

        [Theory]
        [MemberData(nameof(EventStream))]
        public void LiteDBCanStoreTaskEvents(ITaskEvent @event)
        {
            // Arrange
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskEventRepository(db);

                // Act
                sut.AddTaskEvent(@event, DefaultExceptionThrowingErrorHandler.Instance);
                var actualEvents = sut.GetAllTaskEventsFor(@event.Task.TaskId).ToList();

                // Assert
                Assert.Equal(@event, actualEvents[0]);
            }
        }

        [Fact]
        public void LiteDBCanStoreSequentialEvents()
        {
            // Arrange
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskEventRepository(db);

                var firstId = Guid.NewGuid();
                var secondId = Guid.NewGuid();
                ITask task = new ToolTask(Guid.NewGuid(), "someTool", "Test task", "This is a task used to test the tasks' persistency solution", "--someArg");
                ITaskEvent firstEvent = new TaskScheduled(task, firstId, Guid.Empty, 1);
                ITaskEvent secondEvent = new TaskCancelled(secondId, task, firstId);

                // Act
                sut.AddTaskEvent(firstEvent, DefaultExceptionThrowingErrorHandler.Instance);
                sut.AddTaskEvent(secondEvent, DefaultExceptionThrowingErrorHandler.Instance);
                var actualTasks = sut.GetAllTaskEventsFor(task.TaskId).ToList();

                // Assert
                Assert.Equal(firstEvent, actualTasks[0]);
                Assert.Equal(secondEvent, actualTasks[1]);
            }
        }

        [Fact]
        public void LiteDBRejectsOutOfOrderEvents()
        {
            // Arrange
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskEventRepository(db);

                var firstId = Guid.NewGuid();
                var secondId = Guid.NewGuid();
                var thirdId = Guid.NewGuid();
                ITask task = new ToolTask(Guid.NewGuid(), "someTool", "Test task", "This is a task used to test the tasks' persistency solution", "--someArg");
                ITaskEvent firstEvent = new TaskScheduled(task, firstId, Guid.Empty, 1);
                ITaskEvent secondEvent = new TaskStarted(secondId, task, firstId, Enumerable.Empty<ITask>().ToImmutableList());
                // note the previous event which still points to firstId
                ITaskEvent thirdEvent = new TaskCancelled(thirdId, task, firstId);

                // Act
                sut.AddTaskEvent(firstEvent, DefaultExceptionThrowingErrorHandler.Instance);
                sut.AddTaskEvent(secondEvent, DefaultExceptionThrowingErrorHandler.Instance);
                var actualTasks = sut.GetAllTaskEventsFor(task.TaskId).ToList();

                // Assert
                var outOfDateException = Assert.Throws<DefaultExceptionThrowingErrorHandler.PreviousEventNotUpToDate>(
                    () => sut.AddTaskEvent(thirdEvent, DefaultExceptionThrowingErrorHandler.Instance));
                Assert.Equal(secondId, outOfDateException.CurrentLastEventID);
                Assert.Equal(thirdEvent, outOfDateException.FailedToAdd);

                var notUniqueException = Assert.Throws<DefaultExceptionThrowingErrorHandler.EventIdNotUnique>(
                    () => sut.AddTaskEvent(secondEvent, DefaultExceptionThrowingErrorHandler.Instance));
                Assert.Equal(secondEvent.Task, ((ITaskEvent)notUniqueException.PreexistingEvent).Task);
                Assert.Equal(secondEvent, notUniqueException.PreexistingEvent);
                Assert.Equal(secondEvent, notUniqueException.FailedToAdd);

                Assert.Equal(firstEvent, actualTasks[0]);
                Assert.Equal(secondEvent, actualTasks[1]);
            }
        }
    }
}
