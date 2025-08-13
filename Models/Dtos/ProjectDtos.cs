namespace SSToDo.Models.Dtos
{
    public class CreateProjectDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }

    }

    public class UpdateProjectDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class ResponseProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
