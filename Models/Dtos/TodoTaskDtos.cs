using SSToDo.Models.Entities;
using SSToDo.Models.Enums;

namespace SSToDo.Models.Dtos
{
    public class ResponseTodoTaskDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public int? AssignedToUserId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public TaskStatusEnums Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
