using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Application.DTOs
{
    public class TodoItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TodoStatus Status { get; set; }
        public string StatusString => Status.ToString();
        public Priority Priority { get; set; }
        public string PriorityString => Priority.ToString();
        public Guid UserId { get; set; }
    }
}
