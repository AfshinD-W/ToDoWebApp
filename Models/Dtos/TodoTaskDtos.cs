using SSToDo.Models.Enums;

namespace SSToDo.Models.Dtos
{
    public class CreateTodoTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriorityEnums Priority { get; set; }
    }

    public class UpdateTodoTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskStatusEnums? Status { get; set; }
        public TaskPriorityEnums? Priority { get; set; }
    }
}
