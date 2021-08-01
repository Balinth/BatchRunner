using BatchRunner.Domain;
using BatchRunner.Domain.Events;
using BatchRunner.Domain.Interfaces;
using BatchRunner.Persistence.LiteDB;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LiteDB;
using System;
using System.IO;
using System.Linq;

namespace BenchmarkLiteDB
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ReflectionDeserialization>();
        }
    }
    public class ReflectionDeserialization
    {
        [Params(100,1000)]
        public int IterationCount;

        [Benchmark]
        public void WithReflection() => ReflectionOrNo(true);
        [Benchmark]
        public void WithoutReflection() => ReflectionOrNo(false);

        public void ReflectionOrNo(bool withReflection)
        {
            using (LiteDatabase db = new(new MemoryStream()))
            {
                var sut = new TaskEventRepository(db, withReflection);

                ITask task = new ToolTask(Guid.NewGuid(), "someTool", "Test task", "This is a task used to test the tasks' persistency solution", "--someArg");
                var prevEventGuid = Guid.Empty;
                for(int i = 0; i < IterationCount; i++)
                {
                    var currentEventGuid = Guid.NewGuid();
                    ITaskEvent taskEvent = new TaskScheduled(task, currentEventGuid, prevEventGuid, i);
                    sut.AddTaskEvent(taskEvent, DefaultExceptionThrowingErrorHandler.Instance);
                    prevEventGuid = currentEventGuid;
                }

                var allEvents = sut.GetAllTaskEvents().ToList();
            }
        }
    }
}
