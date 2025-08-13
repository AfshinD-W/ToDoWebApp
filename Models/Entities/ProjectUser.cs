namespace SSToDo.Models.Entities
{
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsAdmin { get; set; }
    }
}
