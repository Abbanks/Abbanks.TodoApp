using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Application.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoItemDto>> GetAllTodosAsync(Guid userId);
        Task<TodoItemDto?> GetTodoByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<TodoItemDto>> GetFilteredTodosAsync(Guid userId, TodoStatus? status, Priority? priority, DateTime? dueDateBefore);
        Task<TodoItemDto> CreateTodoAsync(CreateTodoDto createTodoDto, Guid userId);
        Task UpdateTodoAsync(UpdateTodoDto updateTodoDto, Guid userId);
        Task DeleteTodoAsync(Guid id, Guid userId);
    }
}
