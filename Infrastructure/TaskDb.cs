using Microsoft.EntityFrameworkCore;
using Domain;

namespace Infrastructure;

public class TaskDb : DbContext
{
    public TaskDb (DbContextOptions<TaskDb> options)
        : base (options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}