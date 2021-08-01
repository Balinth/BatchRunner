module DomainModel

open System

type ToolLocation =
    | FileSystemPath of string
    | Url of string
    | Repository of repositoryUrl:string * commitHash: string
    

type Tool = {
    Name: string
    Location: ToolLocation
}

type ToolTask = {
    Name: string
    Id: Guid
    Tool: Tool
    Arguments: string list
}

type InnerTask = {
    Name: string
    Id: Guid
    Description: string
}

type Task =
    | ToolTask of ToolTask
    | InnerTask of InnerTask

type Command =
    | StartTask of ToolTask
    | CancelTask of ToolTask

type TaskEvent =
    | TaskStarted of startedAt: DateTime * expectedSubTasks:Task list
    | TaskFinished
    | TaskFailed

type Event =
    | TaskEvent of Task * TaskEvent

type TaskAggregate = {
    Task: Task
    SubTasks: TaskAggregate list
}

type Aggregate =
    | TaskAggregate of TaskAggregate

let update aggregate event =
