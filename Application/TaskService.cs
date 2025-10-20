using Microsoft.AspNetCore.Http;

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
    public async Task<IResult> GetTask();
    public async Task<IResult> GetAllTasks();
    public async Task<IResult> GetCompleteTasks();
    public async Task<IResult> AddTask();
    public async Task<IResult> UpdateTask();
    public async Task<IResult> DeleteTask();
}
}
