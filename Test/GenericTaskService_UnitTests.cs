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

    [Test]
    public async Task GetAllTasks_ShouldReturnCorrectTasksList()
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
        okResult!.Value.Should().BeEquivalentTo(tasksList);
    }
    
    // test the order of the tasks based on complete status and priority
    // order by priority
    [Test]
    public async Task GetAllTasks_ShouldReturnListOrderedByPriority()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        int numTasks = 5;
        for (int i = 1; i <= numTasks; i++)
        {
            tasksList.Add(new TaskItem { Id = i, Name = $"task {i}", IsComplete = false, Priority = numTasks+1-i });
        }
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value.Should().BeInDescendingOrder(t => t.Priority);
    }
    
    [Test]
    public async Task GetAllTasks_ShouldStartWithTaskWithPriorityOf1()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        int numTasks = 5;
        for (int i = 1; i <= numTasks; i++)
        {
            tasksList.Add(new TaskItem { Id = i, Name = $"task {i}", IsComplete = false, Priority = numTasks+1-i });
        }
        var orderedTaskList = tasksList.OrderBy(t => t.Priority).ToList();
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(orderedTaskList));

        // act
        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value!.ElementAt(0).Should().BeEquivalentTo(new TaskItem { Id = 5, Name = $"task 5", IsComplete = false, Priority = 1 });
    }
    
    // order by IsComplete status
    [Test]
    public async Task GetAllTasks_ShouldStartWithTaskWithIsCompleteFalse()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        int numTasks = 3;
        for (int i = 1; i <= numTasks; i++)
        {
            if (i != 3)
            {
                tasksList.Add(new TaskItem { Id = i, Name = $"task {i}", IsComplete = true, Priority = 1 });
            }
            else
            {
                tasksList.Add(new TaskItem { Id = i, Name = $"task {i}", IsComplete = false, Priority = 1 });
            }
        }
        var orderedTaskList = tasksList.OrderBy(t => t.IsComplete).ToList();
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(orderedTaskList));

        // act
        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value!.ElementAt(0).Should().BeEquivalentTo(new TaskItem { Id = 3, Name = "task 3", IsComplete = false, Priority = 1 });
    }
    
    // order by complete status and then priority
    [Test]
    public async Task GetAllTasks_ShouldBeOrderedByIsCompleteThenPriority()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>
        {
            new TaskItem { Id = 1, Name = "task 1", IsComplete = false, Priority = 2 },
            new TaskItem { Id = 2, Name = "task 2", IsComplete = true, Priority = 1 },
            new TaskItem { Id = 3, Name = "task 3", IsComplete = true, Priority = 2 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetAllTasks();
        var okResult = result as Ok<List<TaskItem>>;

        // assert
        okResult!.Value.Should().BeEquivalentTo(tasksList);
    }

    [Test]
    public async Task GetCompleteTasks_ShouldReturnOnlyCompleteTasks()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>
        {
            new TaskItem { Id = 1, Name = "task 1", IsComplete = true, Priority = 1 },
            new TaskItem { Id = 2, Name = "task 2", IsComplete = true, Priority = 1 },
            new TaskItem { Id = 3, Name = "task 3", IsComplete = true, Priority = 2 }
        };
        _mockRepo.Setup(r => r.GetCompleteAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetCompleteTasks();
        var okResult = result as Ok<List<TaskItem>>;
        
        // assert
        okResult!.Value.Should().BeEquivalentTo(tasksList);
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnOk_WhenDbReturnsList()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>
        {
            new TaskItem { Id = 1, Name = "task 1", IsComplete = false, Priority = 1 },
            new TaskItem { Id = 2, Name = "task 2", IsComplete = false, Priority = 1 },
            new TaskItem { Id = 3, Name = "task 3", IsComplete = true, Priority = 2 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetAllTasks();
        
        // assert
        result.Should().BeOfType<Ok<List<TaskItem>>>();
    }
    
    [Test]
    public async Task GetAllTasks_ShouldReturnBadRequest_WhenDbReturnsNull()
    {
        // arrange
        List<TaskItem>? tasksList = null;
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList!));

        // act
        var result = await _service.GetAllTasks();
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task GetAllTasks_ShouldReturnBadRequest_WhenDbReturnsEmptyList()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        _mockRepo.Setup(r => r.GetAllAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetAllTasks();
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task GetTasks_ShouldReturnOk_WhenDbReturnsTaskItem()
    {
        // arrange
        int id = 1;
        TaskItem task = new TaskItem { Id=id, Name="task", IsComplete=false, Priority=1 };
        _mockRepo.Setup(r => r.GetByIdAsync(id)).Returns(Task.FromResult(task));

        // act
        var result = await _service.GetTask(id);
        
        // assert
        result.Should().BeOfType<Ok<TaskItem>>();
    }
    
    [Test]
    public async Task GetTasks_ShouldReturnBadRequest_WhenDbReturnsNull()
    {
        // arrange
        int id = 1;
        TaskItem? task = null;
        _mockRepo.Setup(r => r.GetByIdAsync(id)).Returns(Task.FromResult(task!));

        // act
        var result = await _service.GetTask(id);
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task GetCompleteTasks_ShouldReturnOk_WhenDbReturnsCompletedList()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>
        {
            new TaskItem { Id=1, Name="task", IsComplete=true, Priority=1 }
        };
        _mockRepo.Setup(r => r.GetCompleteAsync()).Returns(Task.FromResult(tasksList.Where(t => t.IsComplete).ToList()));

        // act
        var result = await _service.GetCompleteTasks();
        
        // assert
        result.Should().BeOfType<Ok<List<TaskItem>>>();
    }
    
    [Test]
    public async Task GetCompleteTasks_ShouldReturnBadRequest_WhenDbReturnsEmptyList()
    {
        // arrange
        List<TaskItem> tasksList = new List<TaskItem>();
        _mockRepo.Setup(r => r.GetCompleteAsync()).Returns(Task.FromResult(tasksList));

        // act
        var result = await _service.GetCompleteTasks();
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
    
    [Test]
    public async Task GetCompleteTasks_ShouldReturnBadRequest_WhenDbReturnsNull()
    {
        // arrange
        List<TaskItem>? tasksList = null;
        _mockRepo.Setup(r => r.GetCompleteAsync()).Returns(Task.FromResult(tasksList!));

        // act
        var result = await _service.GetCompleteTasks();
        
        // assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}