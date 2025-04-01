using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Enums;

namespace Abbanks.TodoApp.Infrastructure.Data.Seeding
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (context.Users.Any() || context.TodoItems.Any())
            {
                return;
            }

            var adminUser = new User
            {
                Id = Guid.Parse("a18be9c0-aa65-4af8-bd17-00bd9344e575"),
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = DateTime.UtcNow
            };

            var regularUser = new User
            {
                Id = Guid.Parse("b24c3fa3-0cb1-44a6-8d9c-96a7045a6b98"),
                Username = "user",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(adminUser, regularUser);
            await context.SaveChangesAsync();

            var todos = new List<TodoItem>
        {
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Complete project setup",
                Description = "Set up the initial project structure and Git repository",
                DueDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.Completed,
                Priority = Priority.High,
                UserId = adminUser.Id,
                CompletedAt = DateTime.UtcNow
            },
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Implement authentication",
                Description = "Add JWT authentication to the API",
                DueDate = DateTime.UtcNow.AddDays(3),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.InProgress,
                Priority = Priority.High,
                UserId = adminUser.Id
            },
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Create todo API endpoints",
                Description = "Implement CRUD operations for todos",
                DueDate = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.NotStarted,
                Priority = Priority.Medium,
                UserId = adminUser.Id
            },
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Buy groceries",
                Description = "Milk, eggs, bread, and vegetables",
                DueDate = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.NotStarted,
                Priority = Priority.Medium,
                UserId = regularUser.Id
            },
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Pay utility bills",
                Description = "Electricity and water bills due by end of month",
                DueDate = DateTime.UtcNow.AddDays(10),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.NotStarted,
                Priority = Priority.Low,
                UserId = regularUser.Id
            },
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Schedule dentist appointment",
                Description = "Call dental office to schedule routine checkup",
                DueDate = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow,
                Status = TodoStatus.NotStarted,
                Priority = Priority.Low,
                UserId = regularUser.Id
            }
        };

            await context.TodoItems.AddRangeAsync(todos);
            await context.SaveChangesAsync();
        }
    }
}
