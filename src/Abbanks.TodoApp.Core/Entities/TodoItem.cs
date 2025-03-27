using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Core.Entities
{
    public class TodoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
        public Priority Priority { get; set; } = Priority.Medium;
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
