using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application;

// define interface for generic task service
public interface ITaskService
{
    Task GetTask();
    Task GetAllTasks();
    Task GetCompleteTasks();
    Task AddTask();
    Task UpdateTask();
    Task DeleteTask();
}

// make "concrete" implementation for in memory database
public class InMemoryTaskService : ITaskService
{
    public async Task<IResult> GetTask(DbContext taskDb, int id, ILogger logger)
    {
        logger.LogInformation($"Requesting task with id {id}");
        // how can I pass a generic db context?
        // it doesn't recognise Tasks. I can get around this by passing the actual
        // implementation of TaskDb, but that defeats the point as I haven't separated 
        // infrastructure from the service
        var task = await taskDb.Tasks.FindAsync(id);

        if (task is null)
        {
            logger.LogWarning($"Could not find task with id {id}");
            return TypedResults.BadRequest($"Could not find task with id {id}");
        }

        logger.LogInformation($"Retrieved task with id {id}");
        return TypedResults.Ok(task);
    }

    public async Task<IResult> GetAllTasks() 
    {
        
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
