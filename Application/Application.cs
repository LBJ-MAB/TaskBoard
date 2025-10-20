using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application;
using Domain;
using Infrastructure;

public class Application : ITaskRequests
{
    private readonly TaskDb _context;
    private readonly ILogger<Application> _logger;
    
    // constructor - dependency injection
    public Application(TaskDb dbContext, ILogger<Application> logger)
    {
        _context = dbContext;
        _logger = logger;
    }

    public async Task<IResult> GetTask(int id)
    {
        _logger.LogInformation($"Requesting task with id {id}");
        var task = await _context.Tasks.FindAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"Could not find task with id {id}");
            return TypedResults.BadRequest($"Could not find task with id {id}");
        }

        _logger.LogInformation($"Retrieved task with id {id}");
        return TypedResults.Ok(task);
    }
    // public async Task GetAllTasks();
    // public async Task GetCompleteTasks();
    // public async Task AddTask();
    // public async Task UpdateTask();
    // public async Task DeleteTask();
}