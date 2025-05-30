namespace SSToDo.Entity
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }


}
