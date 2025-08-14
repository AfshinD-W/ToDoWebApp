using SSToDo.Models.Enums;

namespace SSToDo.Models.Entities
{
    public class TodoTask
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public int? AssignedToUserId { get; set; }
        public User AssignedToUser { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public TaskStatusEnums Status { get; set; }
        public TaskPriorityEnums Priority { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
