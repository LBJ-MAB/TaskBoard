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
    public async Task AddTask_ShouldReturnCreated()
    {
        // arrange
        var task = new TaskItem { Name = "test", IsComplete = false, Priority = 1 };
        var result = await _service.AddTask(task);

        // assert
        result.Should().BeOfType<Created>();
    }
    
    [TearDown]
    public void TearDown()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }
    
}