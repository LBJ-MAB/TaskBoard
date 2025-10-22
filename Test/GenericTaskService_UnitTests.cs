using Moq;
using Infrastructure;
using Application;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Test;

[TestFixture]
public class GenericTaskService_UnitTests
{
    private Mock<ITaskRepository> _mockRepo;
    private Mock<ILogger<GenericTaskService>> _mockLogger;
    private GenericTaskService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<GenericTaskService>>();
        _service = new GenericTaskService(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetTask_ShouldReturnCorrectTask()
    {
        // arrange
        var id = 7;
        var task = new TaskItem { Id = id, Name = "enn *w0iwnm aa//al ", IsComplete = true, Priority = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).Returns(Task.FromResult(task));
        
        // Act
        var result = await _service.GetTask(id);
        var okResult = result as Ok<TaskItem>;

        // assert
        okResult!.Value.Should().NotBeNull().And.Be(task);
    }

    [Test]
    public async Task GetAllTasks_ShouldHaveCorrectLength()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        int numTasks = 5;
        for (int i = 1; i <= numTasks; i++)
        {
            tasksList.Add(new TaskItem { Id = i, Name = $"task {i}", IsComplete = false, Priority = 1 });
        }
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value.Should().HaveCount(numTasks);
    }
}