using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public interface ITaskRepository
{
    // get all async
    Task<IEnumerable<TaskItem>> GetAllAsync();
    // get by id
    Task<TaskItem> GetByIdAsync(int id);
    // add
    Task AddAsync(TaskItem task);
    // delete
    Task DeleteAsync(TaskItem task);
    // save changes
    Task SaveChangesAsync();
}

// make concrete implementation for an in memory database
public class InMemoryTaskRepository : ITaskRepository
{
    private TaskDb _context;
    public InMemoryTaskRepository(TaskDb context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync();
    }

    public async Task<TaskItem> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        _context.SaveChangesAsync();
    }
}
