namespace Abbanks.TodoApp.Application.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserDto User { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
