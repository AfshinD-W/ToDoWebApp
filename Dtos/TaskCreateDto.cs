using SSToDo.Models.Enums;

namespace SSToDo.Dtos
{
    public class TaskCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public int AssignedToUserId { get; set; }
        public int DurationDays { get; set; }
        public Priority Priority { get; set; }
    }

}
