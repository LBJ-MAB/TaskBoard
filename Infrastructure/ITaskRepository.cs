using Domain;

namespace Infrastructure;

public interface ITaskRepository
{
    // get all async
    Task<IEnumerable<TaskItem>> GetAllAsync();
    // get by id
    Task<TaskItem> GetByIdAsync(int id);
    // add
    Task AddAsync(TaskItem task);
    // update
    Task UpdateAsync(int id, TaskItem inputTask);
    // delete
    Task DeleteAsync(int id);
}