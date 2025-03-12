using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Abbanks.TodoApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
        }
    }
}
