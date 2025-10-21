using Microsoft.EntityFrameworkCore;    // for UseInMemoryDatabase()
using Application;                      // for TaskService
using Domain;                           // for TaskItem
using Infrastructure;                   // for TaskDb
using Microsoft.AspNetCore.Mvc; 
using Serilog;                          // for logger configuration

var builder = WebApplication.CreateBuilder(args);
// making an in-memory database ->
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
var serilogLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
Log.Logger = serilogLogger;
builder.Host.UseSerilog();

// adding tasks service
builder.Services.AddScoped<ITaskService, InMemoryTaskService>();

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

tasks.MapGet("/", async (ITaskService service) =>
{
    var result = await service.GetAllTasks();
    return result;
});         
tasks.MapGet("/{id}", async (int id, ITaskService service) =>
{
    var result = await service.GetTask(id);
    return result;
}); 
tasks.MapGet("/complete", async (ITaskService service) =>
{
    var result = await service.GetCompleteTasks();
    return result;
});  
tasks.MapPost("/", async (TaskItem task, ITaskService service) =>
{
    var result = await service.AddTask(task);
    return result;
});   
tasks.MapPut("/{id}", async (int id, TaskItem inputTask, ITaskService service) =>
{
    var result = await service.UpdateTask(id, inputTask);
    return result;
});    
tasks.MapDelete("/{id}", async (int id, ITaskService service) =>
{
    var result = await service.DeleteTask(id);
    return result;
});   

app.Run();