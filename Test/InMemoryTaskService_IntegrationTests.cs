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

    [Test]
    public async Task DeleteTask_ShouldReturnBadRequest_WhenNoTaskWithGivenId()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.DeleteTask(2);

        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task DeleteTask_ShouldReturnNoContent_WhenTaskDeleted()
    {
        // arrange
        var task = new TaskItem {  Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.DeleteTask(1);

        // assert
        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task AddTask_ShouldReturnCorrectTask_WhenTaskAdded()
    {
        // arrange
        var task = new TaskItem { Name = "make integration test", IsComplete = false, Priority = 1 };
        var result = await _service.AddTask(task);
        var createdResult = result as Created<TaskItem>;

        // assert
        createdResult.Should().NotBeNull();
        createdResult!.Value.Should().BeEquivalentTo(task);
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnCorrectLength()
    {
        // arrange
        var numTasksToBeAdded = 5;
        for (int taskNum = 0; taskNum < numTasksToBeAdded; taskNum++)
        {
            var task = new TaskItem { Name = $"task {taskNum}", IsComplete = false, Priority = 1 };
            var addTask = await _service.AddTask(task);
        }

        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value.Should().HaveCount(numTasksToBeAdded);
    }

    [Test]
    public async Task GetTask_ShouldReturnCorrectTask()
    {
        // arrange
        var task = new TaskItem { Name = "task", IsComplete = false, Priority = 1 };
        var addTask = await _service.AddTask(task);
        var result = await _service.GetTask(1);
        var okResult = result as Ok<TaskItem>;

        // assert
        okResult!.Value.Should().BeEquivalentTo(task);
    }

    [Test]
    public async Task GetCompleteTasks_ShouldReturnCorrectTasksAndLength()
    {
        // arrange
        var numTasksToAdd = 10;
        var numCompletedTasks = 5;
        for (int taskNum = 0; taskNum < numTasksToAdd; taskNum++)
        {
            if (taskNum < numCompletedTasks)
            {
                var task = new TaskItem { Name = $"Task {taskNum}", IsComplete = true, Priority = 1 };
                var addTask = await _service.AddTask(task);
            }
            else
            {
                var task = new TaskItem { Name = $"Task {taskNum}", IsComplete = false, Priority = 1 };
                var addTask = await _service.AddTask(task);
            }
        }

        var result = await _service.GetCompleteTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value.Should().HaveCount(numCompletedTasks).And.AllSatisfy(x => x.IsComplete.Should().Be(true));
    }

    [Test]
    public async Task GetTask_ShouldReturnUpdatedTask_WhenTaskUpdated()
    {
        // arrange
        var numTasks = 3;
        for (int i = 0; i < numTasks; i++)
        {
            var task = new TaskItem { Name = $"task {i}", IsComplete = false, Priority = 1 };
            var addTask = await _service.AddTask(task);
        }

        var updatedTaskId = 2;
        TaskItem updatedTask = new TaskItem { Id = updatedTaskId, Name = $"task {updatedTaskId}", IsComplete = true, Priority = 1 };
        var updateTask = await _service.UpdateTask(updatedTaskId, updatedTask);

        var result = await _service.GetTask(updatedTaskId);
        var okResult = result as Ok<TaskItem>;

        // assert
        okResult!.Value.Should().BeEquivalentTo(updatedTask);
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnCorrectLength_WhenTaskIsDeleted()
    {
        // arrange
        var numTasks = 4;
        for (int i = 0; i < numTasks; i++)
        {
            var task = new TaskItem { Name = $"task {i}", IsComplete = false, Priority = 1 };
            var addTask = await _service.AddTask(task);
        }

        var resultBeforeDeleting = await _service.GetAllTasks();
        var okResultBeforeDeleting = resultBeforeDeleting as Ok<List<TaskItem>>;
        
        var deletedTaskId = 2;
        var deleteTask = await _service.DeleteTask(deletedTaskId);
        var resultAfterDeleting = await _service.GetAllTasks();
        var okResultAfterDeleting = resultAfterDeleting as Ok<List<TaskItem>>;
        
        // assert
        okResultBeforeDeleting!.Value.Should().HaveCount(numTasks);
        okResultAfterDeleting!.Value.Should().HaveCount(numTasks-1).And.NotContain(task => task.Id == deletedTaskId);
    }
    
    
    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }
}