using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain;
using Infrastructure;

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

var app = builder.Build();
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

// implementing MapGroup for simplification:
var tasks = app.MapGroup("/tasks");

// using methods rather than lambdas
tasks.MapGet("/", GetAllTasks);         // method for getting all tasks
tasks.MapGet("/{id}", GetTask);         // method for getting a single task
tasks.MapGet("/complete", GetCompleteTasks);    // method for getting completed tasks
tasks.MapPost("/", AddTask);            // method for adding a task to the list
tasks.MapPut("/{id}", UpdateTask);      // method for updating a task
tasks.MapDelete("/{id}", DeleteTask);   // method for deleting a task

// get all tasks
static async Task<IResult> GetAllTasks(TaskDb db)
{
    return TypedResults.Ok(await db.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync());
}

// get a single task
static async Task<IResult> GetTask(TaskDb db, int id)
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return TypedResults.BadRequest();
    }

    return TypedResults.Ok(task);
}

// get all complete tasks
static async Task<IResult> GetCompleteTasks(TaskDb db)
{
    return TypedResults.Ok(await db.Tasks.Where(t => t.IsComplete).OrderBy(t => t.Priority).ToListAsync());
}

// add a task to the list
static async Task<IResult> AddTask(TaskDb db, TaskItem task)
{
    db.Tasks.Add(task);

    await db.SaveChangesAsync();

    return TypedResults.Created($"/tasks/{task.Id}", task);
}

// update a task in the list
static async Task<IResult> UpdateTask(TaskDb db, TaskItem inputTask, int id)
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return TypedResults.BadRequest();
    }

    task.Name = inputTask.Name;
    task.IsComplete = inputTask.IsComplete;
    task.Priority = inputTask.Priority;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

// delete a task
static async Task<IResult> DeleteTask(TaskDb db, int id)
{
    var task = await db.Tasks.FindAsync(id);

    if (task is null)
    {
        return TypedResults.BadRequest();
    }

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

app.Run();