using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public interface ITaskRepository
{
    // get all async
    Task<List<TaskItem>> GetAllAsync();
    // get by id
    Task<TaskItem> GetByIdAsync(int id);
    // get complete
    Task<List<TaskItem>> GetCompleteAsync();
    // add
    Task AddAsync(TaskItem task);
    // delete
    Task DeleteAsync(TaskItem task);
    // save changes
    Task SaveChangesAsync();
}

// make concrete implementation for database
public class DbTaskRepository : ITaskRepository
{
    private TaskDb _context;
    public DbTaskRepository(TaskDb context)
    {
        _context = context;
    }
    
    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.OrderBy(t => t.IsComplete).ThenBy(t => t.Priority).ToListAsync();
    }

    public async Task<TaskItem> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<List<TaskItem>> GetCompleteAsync()
    {
        return await _context.Tasks.Where(t => t.IsComplete).ToListAsync();
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
