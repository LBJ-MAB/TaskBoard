using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure;

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
    public async Task<IResult> GetCompleteTasks() 
    {
        
    }
    public async Task<IResult> AddTask() 
    {
        
    }
    public async Task<IResult> UpdateTask() 
    {
        
    }
    public async Task<IResult> DeleteTask() 
    {
        
    }
}
}
