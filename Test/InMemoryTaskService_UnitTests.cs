using FluentAssertions; 
using Domain;
using Moq;
using Application;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // for ITaskService and InMemoryTaskService
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults; // for IResult

namespace Test;

// define repository interface
public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
}

// implement repository
public class EfTaskRepository : ITaskRepository
{
    private readonly TaskDb _db;

    public EfTaskRepository(TaskDb db)
    {
        _db = db;
    }

    public async Task AddAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
    }
}

// refactor service to use repository
public class InMemoryTaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly ILogger<InMemoryTaskService> _logger;

    public InMemoryTaskService(ITaskRepository repo, ILogger<InMemoryTaskService> logger)
    {
        _repo = repo;
        _logger = logger;
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
}

public class InMemoryTaskServiceUnitTests
{
    [SetUp]
    public void Setup()
    {
    }

    // [Test]
    // public async Task AddTask_ShouldImplementIResult()
    // {
    //     // arrange
    //     var task = new TaskItem { Name = "Test Task 1", IsComplete = false, Priority = 1 };
    //     var result = await service.AddTask(task);
    //
    //     // assert
    //     result.Should().BeAssignableTo<IResult>();
    // }

    [Test]
    public async Task AddTask_ShouldReturnCreated()
    {
        // arrange
        var repoMock = new Mock<ITaskRepository>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<InMemoryTaskService>>();
        var service = new InMemoryTaskService(repoMock.Object, loggerMock.Object);
        
        var task = new TaskItem { Name = "test task", IsComplete = false, Priority = 2 };
        var result = await service.AddTask(task);

        // assert
        result.Should().BeOfType<Created>();
    }
}