using Application;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Test;

[TestFixture]
public class InMemoryTaskServiceIntegrationTests
{
    private InMemoryTaskService _service;
    private TaskDb _db;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TaskDb>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _db = new TaskDb(options);
        
        var loggerMock = new Mock<ILogger<InMemoryTaskService>>();
        _service = new InMemoryTaskService(_db, loggerMock.Object);
    }

    [Test]
    public async Task AddTask_ShouldReturnCreated_WhenTaskAdded()
    {
        // arrange
        var task = new TaskItem { Name = "test", IsComplete = false, Priority = 1 };
        var result = await _service.AddTask(task);

        // assert
        result.Should().BeOfType<Created<TaskItem>>();
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnBadRequest_WhenNoTasks()
    {
        // arrange
        var result = await _service.GetAllTasks();

        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnOk_WhenAtLeastOneTask()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetAllTasks();

        // assert
        result.Should().BeOfType<Ok<List<TaskItem>>>();
    }

    [Test]
    public async Task GetTask_ShouldReturnBadRequest_WhenNoTaskWithGivenId()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetTask(2);
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }

    [Test]
    public async Task GetTask_ShouldReturnOk_WhenTaskWithGivenId()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetTask(1);
        
        // assert
        result.Should().BeOfType<Ok<TaskItem>>();
    }

    [Test]
    public async Task GetCompleteTasks_ShouldReturnBadRequest_WhenNoCompleteTasks()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetCompleteTasks();
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task GetCompleteTasks_ShouldReturnOk_WhenCompleteTasks()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = true, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetCompleteTasks();
        
        // assert
        result.Should().BeOfType<Ok<List<TaskItem>>>();
    }

    [Test]
    public async Task UpdateTask_ShouldReturnBadRequest_WhenNoTaskWithGivenId()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.UpdateTask(2, new TaskItem {Name = "task", IsComplete = true, Priority = 1});

        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task UpdateTask_ShouldReturnNoContent_WhenTaskUpdated()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.UpdateTask(1, new TaskItem {Name = "task", IsComplete = true, Priority = 1});

        // assert
        result.Should().BeOfType<NoContent>();
    }
    
    
    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }
    
}