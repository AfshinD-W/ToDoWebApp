using System.Text.Json.Serialization;

namespace SSToDo.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<TodoTask> Tasks { get; set; }
    }
}
