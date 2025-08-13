using SSToDo.Models.Enums;

namespace SSToDo.Models.Entities
{
    public class TaskHistory
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TodoTask TodoTask { get; set; }

        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; }

        public TaskStatusEnums OldStatus { get; set; }
        public TaskStatusEnums NewStatus { get; set; }

        public DateTime ChangedAt { get; set; }
    }

}
