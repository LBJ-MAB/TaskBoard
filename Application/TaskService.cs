using Microsoft.AspNetCore.Http;            // for IResult
using Microsoft.EntityFrameworkCore;        // for .ToListAsync()???
using Microsoft.Extensions.Logging;         // for logger
using Infrastructure;                       // for TaskDb
using Domain;                               // for TaskItem

namespace Application;

// define interface for generic task service
public interface ITaskService
{
    Task<IResult> GetTask(int id);
    Task<IResult> GetAllTasks();
    Task<IResult> GetCompleteTasks();
    Task<IResult> AddTask(TaskItem task);
    Task<IResult> UpdateTask(int id, TaskItem inputTask);
    Task<IResult> DeleteTask(int id);
}

// make "concrete" implementation for in memory database
public class InMemoryTaskService : ITaskService
{
    private readonly TaskDb _db;
    private readonly ILogger _logger;
    
    public InMemoryTaskService(TaskDb db, ILogger<InMemoryTaskService> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    public async Task<IResult> GetTask(int id)
    {
        _logger.LogInformation($"Requesting task with id {id}");
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"Could not find task with id {id}");
            return TypedResults.BadRequest($"Could not find task with id {id}");
        }

        _logger.LogInformation($"Retrieved task with id {id}");
        return TypedResults.Ok(task);
    }
    public async Task<IResult> GetAllTasks()  
    {
        // log the beginning of the request
        _logger.LogInformation("Requesting all tasks");
        // get all tasks - only this line to be replaced with repo stuff - data access
        var tasks = await _db.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync();
        if (tasks == null || !tasks.Any())
        {
            _logger.LogWarning("No tasks were found");
            return TypedResults.BadRequest("no tasks were found");
        }
        _logger.LogInformation($"Retrieved {tasks.Count} tasks");
        return TypedResults.Ok(tasks);
    }
    public async Task<IResult> GetCompleteTasks() 
    {
        _logger.LogInformation($"Requesting all complete tasks");
        var completeTasks = await _db.Tasks.Where(t => t.IsComplete).OrderBy(t => t.Priority).ToListAsync();
        if (completeTasks == null || !completeTasks.Any())
        {
            _logger.LogInformation("Could not find any complete tasks");
            return TypedResults.BadRequest("Could not find any complete tasks");
        }

        _logger.LogInformation($"Retrieved {completeTasks.Count} complete tasks");
        return TypedResults.Ok(completeTasks);
    }
    public async Task<IResult> AddTask(TaskItem task) 
    {
        _logger.LogInformation("adding task");
        try
        {
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
        }
        catch
        {
            _logger.LogWarning("unable to add task");
            return TypedResults.BadRequest("unable to add task");
        }

        _logger.LogInformation("successfully added task");
        return TypedResults.Created($"/tasks/{task.Id}", task);
    }
    public async Task<IResult> UpdateTask(int id, TaskItem inputTask) 
    {
        _logger.LogInformation($"updating task {id}");
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"unable to update task {id}");
            return TypedResults.BadRequest($"unable to update task {id}");
        }

        task.Name = inputTask.Name;
        task.IsComplete = inputTask.IsComplete;
        task.Priority = inputTask.Priority;

        await _db.SaveChangesAsync();

        _logger.LogInformation($"successfully updated task {id}");
        return TypedResults.NoContent();
    }
    public async Task<IResult> DeleteTask(int id) 
    {
        _logger.LogInformation($"attempting to delete task {id}");
        var task = await _db.Tasks.FindAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"unable to find task with id {id}");
            return TypedResults.BadRequest($"unable to find task with id {id}");
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"successfully deleted task {id}");
        return TypedResults.NoContent();
    }
}

// make generic implementation making use of repository
public class GenericTaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger _logger;
    
    public GenericTaskService(ITaskRepository repo, ILogger<GenericTaskService> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    
    public async Task<IResult> GetTask(int id)
    {
        _logger.LogInformation($"Requesting task with id {id}");
        var task = await _repo.GetByIdAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"Could not find task with id {id}");
            return TypedResults.BadRequest($"Could not find task with id {id}");
        }

        _logger.LogInformation($"Retrieved task with id {id}");
        return TypedResults.Ok(task);
    }
    public async Task<IResult> GetAllTasks()  
    {
        // log the beginning of the request
        _logger.LogInformation("Requesting all tasks");
        // get all tasks - only this line to be replaced with repo stuff - data access
        var tasks = await _repo.GetAllAsync();
        if (tasks == null || !tasks.Any())
        {
            _logger.LogWarning("No tasks were found");
            return TypedResults.BadRequest("no tasks were found");
        }
        _logger.LogInformation($"Retrieved {tasks.Count} tasks");
        return TypedResults.Ok(tasks);
    }
    public async Task<IResult> GetCompleteTasks() 
    {
        _logger.LogInformation($"Requesting all complete tasks");
        var completeTasks = await _repo.GetCompleteAsync();
        if (completeTasks == null || !completeTasks.Any())
        {
            _logger.LogInformation("Could not find any complete tasks");
            return TypedResults.BadRequest("Could not find any complete tasks");
        }

        _logger.LogInformation($"Retrieved {completeTasks.Count} complete tasks");
        return TypedResults.Ok(completeTasks);
    }
    public async Task<IResult> AddTask(TaskItem task) 
    {
        _logger.LogInformation("adding task");
        try
        {
            await _repo.AddAsync(task);
        }
        catch
        {
            _logger.LogWarning("unable to add task");
            return TypedResults.BadRequest("unable to add task");
        }

        _logger.LogInformation("successfully added task");
        return TypedResults.Created($"/tasks/{task.Id}", task);
    }
    public async Task<IResult> UpdateTask(int id, TaskItem inputTask) 
    {
        _logger.LogInformation($"updating task {id}");
        var task = await _repo.GetByIdAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"unable to update task {id}");
            return TypedResults.BadRequest($"unable to update task {id}");
        }

        task.Name = inputTask.Name;
        task.IsComplete = inputTask.IsComplete;
        task.Priority = inputTask.Priority;

        await _repo.SaveChangesAsync();

        _logger.LogInformation($"successfully updated task {id}");
        return TypedResults.NoContent();
    }
    public async Task<IResult> DeleteTask(int id) 
    {
        _logger.LogInformation($"attempting to delete task {id}");
        var task = await _repo.GetByIdAsync(id);

        if (task is null)
        {
            _logger.LogWarning($"unable to find task with id {id}");
            return TypedResults.BadRequest($"unable to find task with id {id}");
        }

        await _repo.DeleteAsync(task);

        _logger.LogInformation($"successfully deleted task {id}");
        return TypedResults.NoContent();
    }
}
