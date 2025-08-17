using SSToDo.Models.Enums;

namespace SSToDo.Models.Entities
{
    public class ProjectUserInvite
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string InviteToken { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
