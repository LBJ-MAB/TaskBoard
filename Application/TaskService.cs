using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure;
using Domain;

namespace Application;

// define interface for generic task service
public interface ITaskService
{
    Task GetTask(int id, DbContext taskDb, ILogger logger);
    Task GetAllTasks();
    Task GetCompleteTasks();
    Task AddTask();
    Task UpdateTask();
    Task DeleteTask();
}

// make "concrete" implementation for in memory database
public class InMemoryTaskService : ITaskService
{
    public async Task<IResult> GetTask(int id, TaskDb db, ILogger logger)
    {
        logger.LogInformation($"Requesting task with id {id}");
        var task = await db.Tasks.FindAsync(id);

        if (task is null)
        {
            logger.LogWarning($"Could not find task with id {id}");
            return TypedResults.BadRequest($"Could not find task with id {id}");
        }

        logger.LogInformation($"Retrieved task with id {id}");
        return TypedResults.Ok(task);
    }
    public async Task<IResult> GetAllTasks(TaskDb db, ILogger logger)  
    {
        // log the beginning of the request
        logger.LogInformation("Requesting all tasks");
        // get all tasks
        var tasks = await db.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync();
        if (tasks == null || !tasks.Any())
        {
            logger.LogWarning("No tasks were found");
            return TypedResults.BadRequest("no tasks were found");
        }
        logger.LogInformation($"Retrieved {tasks.Count} tasks");
        return TypedResults.Ok(tasks);
    }
    public async Task<IResult> GetCompleteTasks(TaskDb db, ILogger logger) 
    {
        logger.LogInformation($"Requesting all complete tasks");
        var completeTasks = await db.Tasks.Where(t => t.IsComplete).OrderBy(t => t.Priority).ToListAsync();
        if (completeTasks == null || !completeTasks.Any())
        {
            logger.LogInformation("Could not find any complete tasks");
            return TypedResults.BadRequest("Could not find any complete tasks");
        }

        logger.LogInformation($"Retrieved {completeTasks.Count} complete tasks");
        return TypedResults.Ok(completeTasks);
    }
    public async Task<IResult> AddTask(TaskItem task, TaskDb db, ILogger logger) 
    {
        logger.LogInformation("adding task");
        try
        {
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
        }
        catch
        {
            logger.LogWarning("unable to add task");
            return TypedResults.BadRequest("unable to add task");
        }

        logger.LogInformation("successfully added task");
        return TypedResults.Created($"/tasks/{task.Id}", task);
    }
    public async Task<IResult> UpdateTask(int id, TaskItem inputTask, TaskDb db, ILogger logger) 
    {
        logger.LogInformation($"updating task {id}");
        var task = await db.Tasks.FindAsync(id);

        if (task is null)
        {
            logger.LogWarning($"unable to update task {id}");
            return TypedResults.BadRequest($"unable to update task {id}");
        }

        task.Name = inputTask.Name;
        task.IsComplete = inputTask.IsComplete;
        task.Priority = inputTask.Priority;

        await db.SaveChangesAsync();

        logger.LogInformation($"successfully updated task {id}");
        return TypedResults.NoContent();
    }
    public async Task<IResult> DeleteTask(int id, TaskDb db, ILogger logger) 
    {
        logger.LogInformation($"attempting to delete task {id}");
        var task = await db.Tasks.FindAsync(id);

        if (task is null)
        {
            logger.LogWarning($"unable to find task with id {id}");
            return TypedResults.BadRequest($"unable to find task with id {id}");
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        logger.LogInformation($"successfully deleted task {id}");
        return TypedResults.NoContent();
    }
}
}
