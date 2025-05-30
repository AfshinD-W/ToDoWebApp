namespace SSToDo.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<Project> CreatedProjects { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; }

        public ICollection<TaskItem> AssignedTasks { get; set; }
    }


}
