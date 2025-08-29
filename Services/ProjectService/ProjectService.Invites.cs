using Microsoft.EntityFrameworkCore;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;

namespace SSToDo.Services.ProjectService
{
    public partial class ProjectService
    {
        //Confirm accept and add member to project
        public async Task<bool> ConfirmInviteAsync(string inviteToken)
        {
            var invite = await _context.ProjectUsersInvite.FirstOrDefaultAsync(i => i.InviteToken == inviteToken);

            if (invite == null)
                return false;

            if (invite.Status != InviteStatus.Pending)
                return false;

            var alreadyExists = await _context.ProjectUsers.AsNoTracking()
            .AnyAsync(pu => pu.ProjectId == invite.ProjectId && pu.UserId == invite.UserId);

            if (alreadyExists)
                return false;

            invite.Status = InviteStatus.Accepted;

            var newProjectUser = new ProjectUser
            {
                ProjectId = invite.ProjectId,
                UserId = invite.UserId,
            };

            await _context.ProjectUsers.AddAsync(newProjectUser);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
