using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Enums;
using Abbanks.TodoApp.Core.Repositories;
using AutoMapper;

namespace Abbanks.TodoApp.Application.Services.Implementations
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public TodoService(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository ?? throw new ArgumentNullException(nameof(todoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TodoItemDto>> GetAllTodosAsync(Guid userId)
        {
            var todos = await _todoRepository.GetAllAsync(userId);
            return _mapper.Map<IEnumerable<TodoItemDto>>(todos);
        }

        public async Task<TodoItemDto?> GetTodoByIdAsync(Guid id, Guid userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null || todo.UserId != userId)
                return null;
            return _mapper.Map<TodoItemDto>(todo);
        }

        public async Task<IEnumerable<TodoItemDto>> GetFilteredTodosAsync(Guid userId, TodoStatus? status, Priority? priority, DateTime? dueDateBefore)
        {
            var todos = await _todoRepository.GetByFilterAsync(userId, status, priority, dueDateBefore);
            return _mapper.Map<IEnumerable<TodoItemDto>>(todos);
        }

        public async Task<TodoItemDto> CreateTodoAsync(CreateTodoDto createTodoDto, Guid userId)
        {
            var todoEntity = new TodoItem
            {
                Title = createTodoDto.Title,
                Description = createTodoDto.Description ?? string.Empty,
                DueDate = createTodoDto.DueDate,
                Status = TodoStatus.NotStarted,
                Priority = createTodoDto.Priority,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdTodo = await _todoRepository.AddAsync(todoEntity);
            return _mapper.Map<TodoItemDto>(createdTodo);
        }

        public async Task UpdateTodoAsync(UpdateTodoDto updateTodoDto, Guid userId)
        {
            var existingTodo = await _todoRepository.GetByIdAsync(updateTodoDto.Id);
            if (existingTodo == null)
                throw new KeyNotFoundException($"Todo with ID {updateTodoDto.Id} not found");

            if (existingTodo.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this todo");

            existingTodo.Title = updateTodoDto.Title;
            existingTodo.Description = updateTodoDto.Description ?? string.Empty;
            existingTodo.DueDate = updateTodoDto.DueDate;
            existingTodo.Priority = updateTodoDto.Priority;

            if (existingTodo.Status != updateTodoDto.Status)
            {
                existingTodo.Status = updateTodoDto.Status;

                if (updateTodoDto.Status == TodoStatus.Completed)
                    existingTodo.CompletedAt = DateTime.UtcNow;
                else
                    existingTodo.CompletedAt = null;
            }

            await _todoRepository.UpdateAsync(existingTodo);
        }

        public async Task DeleteTodoAsync(Guid id, Guid userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {id} not found");

            if (todo.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this todo");

            await _todoRepository.DeleteAsync(id);
        }
    }
}
