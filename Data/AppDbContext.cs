using Microsoft.EntityFrameworkCore;
using SSToDo.Models.Entities;
using SSToDo.Models.EntityConfigurations;

namespace SSToDo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUserInvite> ProjectUsersInvite { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<TodoTask> TodoTasks { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfigurations).Assembly);
        }
    }
}
