using System.Text.Json.Serialization;

namespace SSToDo.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ConfirmedEmail { get; set; }
        public string Password { get; set; }
        public int? Age { get; set; }
        public string? ImagePath { get; set; }

        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<TodoTask> AssignedTasks { get; set; }
    }
}
