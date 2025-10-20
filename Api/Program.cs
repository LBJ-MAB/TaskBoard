using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain;
using Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// dependency injection
builder.Services.AddDbContext<TaskDb>(opt => opt.UseInMemoryDatabase("Tasks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TaskBoardAPI";
    config.Title = "TaskBoardAPI v1";
    config.Version = "v1";
});
// configure serilog
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
Log.Logger = logger;
builder.Host.UseSerilog();

// build the web application
var app = builder.Build();
// use swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TaskBoardAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// middleware for logging to console - view in Run window in Rider
// app.UseMiddleware<RequestLoggingMiddleware>();

// implementing MapGroup for simplification:
var tasks = app.MapGroup("/tasks");
// using methods rather than lambdas
tasks.MapGet("/", GetAllTasks);         // method for getting all tasks
tasks.MapGet("/{id}", GetTask);         // method for getting a single task
tasks.MapGet("/complete", GetCompleteTasks);    // method for getting completed tasks
tasks.MapPost("/", AddTask);            // method for adding a task to the list
tasks.MapPut("/{id}", UpdateTask);      // method for updating a task
tasks.MapDelete("/{id}", DeleteTask);   // method for deleting a task


// ---------- THESE ALL NEED MIGRATING TO APPLICATION -----------
// get all tasks
static async Task<IResult> GetAllTasks(TaskDb db, ILogger<Program> loggerInput)
{
    // log the beginning of the request
    loggerInput.LogInformation("Requesting all tasks");
    // get tasks
    var tasks = await db.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync();
    if (tasks == null || !tasks.Any())
    {
        loggerInput.LogWarning("No tasks were found");
        return TypedResults.BadRequest("no tasks were found");
    }
    loggerInput.LogInformation($"Retrieved {tasks.Count} tasks");
    return TypedResults.Ok(tasks);
}
// get a single task
static async Task<IResult> GetTask(TaskDb db, int id, ILogger<Program> loggerInput)
{
    loggerInput.LogInformation($"Requesting task with id {id}");
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        loggerInput.LogWarning($"Could not find task with id {id}");
        return TypedResults.BadRequest($"Could not find task with id {id}");
    }

    loggerInput.LogInformation($"Retrieved task with id {id}");
    return TypedResults.Ok(task);
}
// get all complete tasks
static async Task<IResult> GetCompleteTasks(TaskDb db, ILogger<Program> loggerInput)
{
    loggerInput.LogInformation($"Requesting all complete tasks");
    var completeTasks = await db.Tasks.Where(t => t.IsComplete).OrderBy(t => t.Priority).ToListAsync();
    if (completeTasks == null || !completeTasks.Any())
    {
        loggerInput.LogInformation("Could not find any complete tasks");
        return TypedResults.BadRequest("Could not find any complete tasks");
    }

    loggerInput.LogInformation($"Retrieved {completeTasks.Count} complete tasks");
    return TypedResults.Ok(completeTasks);
}
// add a task to the list
static async Task<IResult> AddTask(TaskDb db, TaskItem task, ILogger<Program> loggerInput)
{
    loggerInput.LogInformation("adding task");
    try
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }
    catch
    {
        loggerInput.LogWarning("unable to add task");
        return TypedResults.BadRequest("unable to add task");
    }

    loggerInput.LogInformation("successfully added task");
    return TypedResults.Created($"/tasks/{task.Id}", task);
}
// update a task in the list
static async Task<IResult> UpdateTask(TaskDb db, TaskItem inputTask, int id, ILogger<Program> loggerInput)
{
    loggerInput.LogInformation($"updating task {id}");
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        loggerInput.LogWarning($"unable to update task {id}");
        return TypedResults.BadRequest($"unable to update task {id}");
    }

    task.Name = inputTask.Name;
    task.IsComplete = inputTask.IsComplete;
    task.Priority = inputTask.Priority;

    await db.SaveChangesAsync();

    loggerInput.LogInformation($"successfully updated task {id}");
    return TypedResults.NoContent();
}
// delete a task
static async Task<IResult> DeleteTask(TaskDb db, int id, ILogger<Program> loggerInput)
{
    loggerInput.LogInformation($"attempting to delete task {id}");
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        loggerInput.LogWarning($"unable to find task with id {id}");
        return TypedResults.BadRequest($"unable to find task with id {id}");
    }

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();

    loggerInput.LogInformation($"successfully deleted task {id}");
    return TypedResults.NoContent();
}

app.Run();