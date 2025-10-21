using FluentAssertions; 
using Domain;
using Moq;
using Application;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // for ITaskService and InMemoryTaskService
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults; // for IResult
using Microsoft.AspNetCore.Mvc; 

namespace Test;

public class InMemoryTaskServiceTests
{
    private Mock<TaskDb> _dbMock;   // mock a task db context
    private Mock<ILogger<InMemoryTaskService>> _loggerMock;     // mock a logger
    private InMemoryTaskService _service;   // service
    
    [SetUp]
    public void Setup()
    {
        // config options for in memory database
        var options = new DbContextOptionsBuilder<TaskDb>()
            .UseInMemoryDatabase(databaseName: "testDb")
            .Options;
        // mock object for TaskDb using options
        _dbMock = new Mock<TaskDb>(options);
        // mock logger to avoid logging during tests
        _loggerMock = new Mock<ILogger<InMemoryTaskService>>();
        // instantiate InMemoryTaskService for testing
        _service = new InMemoryTaskService(_dbMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddTask_ShouldImplementIResult()
    {
        // arrange
        var task = new TaskItem { Name = "Test Task 1", IsComplete = false, Priority = 1 };
        var result = await _service.AddTask(task);

        // assert
        result.Should().BeAssignableTo<IResult>();
    }

    [Test]
    public async Task AddTask_ShouldReturnCreated()
    {
        // arrange
        var task = new TaskItem { Name = "test task", IsComplete = false, Priority = 2 };
        var result = await _service.AddTask(task);

        // assert
        result.Should().BeOfType<Created>();
    }
}