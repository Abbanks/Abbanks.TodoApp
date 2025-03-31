using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Enums;
using Abbanks.TodoApp.Core.Repositories;
using Abbanks.TodoApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Abbanks.TodoApp.Infrastructure.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TodoItem>> GetAllAsync(Guid userId)
        {
            return await _context.TodoItems
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAsync(Guid id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task<IEnumerable<TodoItem>> GetByFilterAsync(Guid userId, TodoStatus? status = null, Priority? priority = null, DateTime? dueDateBefore = null)
        {
            var query = _context.TodoItems.Where(t => t.UserId == userId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (dueDateBefore.HasValue)
                query = query.Where(t => t.DueDate <= dueDateBefore.Value);

            return await query
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TodoItem> AddAsync(TodoItem todoItem)
        {
            await _context.TodoItems.AddAsync(todoItem);
            await _context.SaveChangesAsync();
            return todoItem;
        }

        public async Task UpdateAsync(TodoItem todoItem)
        {
            _context.Entry(todoItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.TodoItems.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> BelongsToUserAsync(Guid id, Guid userId)
        {
            return await _context.TodoItems.AnyAsync(t => t.Id == id && t.UserId == userId);
        }
    }
}
