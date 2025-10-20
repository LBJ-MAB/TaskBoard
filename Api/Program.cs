using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Application;
using Domain;
using Infrastructure;           // I'm not sure program should be using this now
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// dependency injection
// WE WILL DEPENDENCY INJECT THE ITaskService -> service here
// making an in-memory data base ->
builder.Services.AddDbContext<TaskDb>(opt => opt.UseInMemoryDatabase("Tasks"));
// adding tasks service
builder.Services.AddSingleton<ITaskService>(new InMemoryTaskService());

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
tasks.MapGet("/", async (ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    await service.GetAllTasks(db, logger);
});         
tasks.MapGet("/{id}", async (int id, ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    // I am passing a TaskDb db here. When I do this across all the MapGet methods 
    // does the program know to work with the same in-memory database because I
    // have given the db the name of "Tasks" at the top? 
    // If so, if I were to create another database also of type TaskDb, how would it 
    // know which TaskDb I am referring to?
    await service.GetTask(id, db, logger);
}); 
tasks.MapGet("/complete", async (ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    await service.GetCompleteTasks(db, logger);
});  
tasks.MapPost("/", async (TaskItem task, ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    await service.AddTask(task, db, logger);
});   
tasks.MapPut("/{id}", async (int id, TaskItem inputTask, ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    await service.UpdateTask(id, inputTask, db, logger);
});    
tasks.MapDelete("/{id}", async (int id, ITaskService service, TaskDb db, Microsoft.Extensions.Logging.ILogger logger) =>
{
    await service.DeleteTask();
});   

app.Run();