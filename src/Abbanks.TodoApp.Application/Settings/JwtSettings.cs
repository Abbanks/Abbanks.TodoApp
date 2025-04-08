namespace Abbanks.TodoApp.Application.Settings
{
    public class JwtSettings
    {
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpirationDays { get; set; }
        public string Secret { get; set; } = string.Empty;
    }
}
