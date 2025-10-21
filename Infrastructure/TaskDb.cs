using Microsoft.EntityFrameworkCore;
using Domain;

namespace Infrastructure;

public class TaskDb : DbContext
{
    public TaskDb (DbContextOptions<TaskDb> options)
        : base (options) { }

    // this is setting up en entity set called Tasks which will be filled with elements of TaskItem
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}