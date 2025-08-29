using Microsoft.EntityFrameworkCore;
using SSToDo.Models.Entities;
using SSToDo.Utilities;

namespace SSToDo.Services.ProjectService
{
    public partial class ProjectService
    {
        //Send emaile to members to add them to project
        public async Task<ServiceResponse<List<int>>> InviteMemberToProjectAsync(List<int> memberIds, int projectId)
        {
            if (memberIds == null)
                return new ServiceResponse<List<int>>("Invalid inputs");

            var project = await _context.Projects
                .Where(p => p.Id == projectId)
                .Include(p => p.ProjectUsers)
                .Select(p => new
                {
                    Project = p,
                    IsAdmin = p.ProjectUsers.Any(u => u.UserId == _userContextService.GetUserId() && u.IsAdmin),
                    ExistingUserIds = p.ProjectUsers.Select(u => u.UserId).ToList()
                }).SingleOrDefaultAsync();

            if (project == null)
                return new ServiceResponse<List<int>>("Project does not exist");
            if (!project.IsAdmin)
                return new ServiceResponse<List<int>>("You are not the admin");

            var newUsersToAdd = new List<ProjectUserInvite>();
            var membersEmailes = await _context.Users.AsNoTracking().Where(u => memberIds.Contains(u.Id)).Select(u => new { u.Id, u.Email }).ToListAsync();


            foreach (var userId in memberIds)
            {
                var hasActiveInvited = await _context.ProjectUsersInvite.AsNoTracking().Where(i => i.ProjectId == projectId && i.UserId == userId && i.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

                if (!project.ExistingUserIds.Contains(userId) && hasActiveInvited == null)
                {
                    var inviteToken = Guid.NewGuid().ToString();
                    var memberEmaile = membersEmailes.Where(u => u.Id == userId).Select(e => e.Email).FirstOrDefault();

                    newUsersToAdd.Add(new ProjectUserInvite
                    {
                        ProjectId = projectId,
                        UserId = userId,
                        InviteToken = inviteToken
                    });

                    await _emaileService.SendEmailAsync(memberEmaile, $"Project Invitation - {project.Project.Title}"
                        , $"You have been invited to join the project **{project.Project.Title}**.\n " +
                            $"If you would like to accept this invitation, please click the confirm.\n " +
                            $"If you did not expect this invitation, you can safely ignore this email."
                        , $"{_configuration["ApplicationSettings:BaseUrl"]}/api/Project/invite?token={inviteToken}");
                }
            }

            if (newUsersToAdd.Any())
            {
                await _context.ProjectUsersInvite.AddRangeAsync(newUsersToAdd);
                await _context.SaveChangesAsync();
            }

            return new ServiceResponse<List<int>>(newUsersToAdd.Select(u => u.UserId).ToList());
        }

        //Remove member from project
        public async Task<ServiceResponse<string>> RemoveMemberFromProjectAsync(List<int> memberIds, int projectId)
        {
            if (memberIds == null || !memberIds.Any())
                return new ServiceResponse<string>("No users provided for removal");

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .SingleOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return new ServiceResponse<string>("Project does not exist");

            var currentUserId = _userContextService.GetUserId();
            var isAdmin = project.ProjectUsers.Any(u => u.UserId == currentUserId && u.IsAdmin);
            if (!isAdmin)
                return new ServiceResponse<string>("You are not the admin");

            var userForDelete = project.ProjectUsers
                 .Where(u => memberIds.Contains(u.UserId))
                 .ToList();

            var notExistsMembers = memberIds.Except(userForDelete.Select(u => u.UserId)).ToList();


            if (notExistsMembers.Any())
                return new ServiceResponse<string>($"Members with ID: {string.Join(", ", notExistsMembers)} are not in project!");

            if (userForDelete.Any(u => u.UserId == currentUserId))
                return new ServiceResponse<string>("You cant delete admin!");

            _context.ProjectUsers.RemoveRange(userForDelete);
            await _context.SaveChangesAsync();

            return new ServiceResponse<string>($"Users with ID: {string.Join(", ", userForDelete.Select(u => u.UserId))} deleted successfully")
            { Data = $"Users with ID: {string.Join(", ", userForDelete.Select(u => u.UserId))} deleted successfully" };
        }

    }
}
