using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Core.Entities
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } 
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TodoStatus Status { get; set; }
        public Priority Priority { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
