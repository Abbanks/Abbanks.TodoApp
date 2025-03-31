using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Core.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoItem>> GetAllAsync(Guid userId);
        Task<TodoItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<TodoItem>> GetByFilterAsync(Guid userId, TodoStatus? status = null, Priority? priority = null, DateTime? dueDateBefore = null);
        Task<TodoItem> AddAsync(TodoItem todoItem);
        Task UpdateAsync(TodoItem todoItem);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> BelongsToUserAsync(Guid id, Guid userId);
    }
}
