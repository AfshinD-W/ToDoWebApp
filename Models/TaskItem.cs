using SSToDo.Models.Enums;
using TaskStatusEnum = SSToDo.Models.Enums.TaskStatus;


namespace SSToDo.Entity
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public Priority Priority { get; set; }
        public int DurationDays { get; set; }

        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Open;

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int AssignedToId { get; set; }
        public User AssignedTo { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

}
