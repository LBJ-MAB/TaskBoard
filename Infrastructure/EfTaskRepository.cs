using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
}

    
public class EfTaskRepository : ITaskRepository
{
    private readonly TaskDb _db;

    public EfTaskRepository(TaskDb db)
    {
        _db = db;
    }

    public async Task AddAsync(TaskItem item)
    {
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();
    }

    // Implement other ITaskRepository methods as needed
}
