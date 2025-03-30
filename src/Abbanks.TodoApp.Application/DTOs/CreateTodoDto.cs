using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Application.DTOs
{
    public class CreateTodoDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; }
    }
}
