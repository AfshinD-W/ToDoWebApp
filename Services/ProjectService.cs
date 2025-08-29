using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;
using SSToDo.Shared;
using SSToDo.Utilities;

namespace SSToDo.Services
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<Project>>> GetProjectsAsync();
        Task<ServiceResponse<List<TodoTask>>> GetProjectTodoTasksAsync(int projectId);
        Task<ServiceResponse<ResponseProjectDto>> CreateProjectAsync(CreateProjectDto project);
        Task<ServiceResponse<ResponseProjectDto>> UpdateProjectAsync(UpdateProjectDto project, int projectId);
        Task<ServiceResponse<List<int>>> InviteMemberToProjectAsync(List<int> memberIds, int projectId);
        Task<bool> ConfirmInviteAsync(string inviteToken);
        Task<ServiceResponse<string>> RemoveMemberFromProjectAsync(List<int> memberIds, int projectId);
        Task<ServiceResponse<string>> DeleteProjectAsync(int projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IHashPasswordService _hashPasswordService;
        private readonly IEmailService _emaileService;
        private readonly IConfiguration _configuration;

        public ProjectService(AppDbContext context, IUserContextService userContextService, IHashPasswordService hashPasswordService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _userContextService = userContextService;
            _emaileService = emailService;
            _configuration = configuration;
        }

        //Get projects
        public async Task<ServiceResponse<List<Project>>> GetProjectsAsync()
        {
            var projects = await _context.Projects.AsNoTracking().Where(u => u.ProjectUsers.Any(pu => pu.UserId == _userContextService.GetUserId())).ToListAsync();
            return new ServiceResponse<List<Project>>(projects);
        }

        //Get project todotasks
        public async Task<ServiceResponse<List<TodoTask>>> GetProjectTodoTasksAsync(int projectId)
        {
            var projectTasks = await _context.TodoTasks
                .Where(t => t.ProjectId == projectId &&
                        t.Project.ProjectUsers.Any(u => u.UserId == _userContextService.GetUserId()))
                .ToListAsync();

            int expierdTaskNum = 0;

            foreach (var todoTask in projectTasks)
            {
                if (todoTask.DueDate < DateTime.UtcNow && todoTask.Status != TaskStatusEnums.Expired)
                {
                    todoTask.Status = TaskStatusEnums.Expired;
                    expierdTaskNum++;
                }
            }

            if (expierdTaskNum > 0)
                await _context.SaveChangesAsync();

            return new ServiceResponse<List<TodoTask>>(projectTasks);
        }

        //Create project
        public async Task<ServiceResponse<ResponseProjectDto>> CreateProjectAsync(CreateProjectDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var newProject = new Project
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedByUserId = _userContextService.GetUserId()
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            var projectUser = new ProjectUser { ProjectId = newProject.Id, UserId = _userContextService.GetUserId(), IsAdmin = true };
            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var responseDto = new ResponseProjectDto
            {
                Id = newProject.Id,
                Title = newProject.Title,
                Description = newProject.Description,
                CreatedByUserId = newProject.CreatedByUserId,
                CreatedAt = newProject.CreatedAt
            };
            return new ServiceResponse<ResponseProjectDto>(responseDto);

        }

        //Update project
        public async Task<ServiceResponse<ResponseProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, int projectId)
        {
            if (dto == null)
                return new ServiceResponse<ResponseProjectDto>("No entry!");

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .Select(p => new
                {
                    Project = p,
                    IsAdmin = p.ProjectUsers.Any(u => u.UserId == _userContextService.GetUserId() && u.IsAdmin)
                }).SingleOrDefaultAsync(p => p.Project.Id == projectId);

            if (project == null)
                return new ServiceResponse<ResponseProjectDto>("Project does not exist");
            if (!project.IsAdmin)
                return new ServiceResponse<ResponseProjectDto>("You are not the admin");

            var entity = project.Project;

            if (!string.IsNullOrEmpty(dto.Title) && dto.Title != entity.Title)
                entity.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description != entity.Description)
                entity.Description = dto.Description;

            await _context.SaveChangesAsync();

            var responseDto = new ResponseProjectDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedAt = entity.CreatedAt
            };
            return new ServiceResponse<ResponseProjectDto>(responseDto);
        }

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

        //Delete project
        public async Task<ServiceResponse<string>> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects.Include(p => p.ProjectUsers)
                .Select(p => new
                {
                    project = p,
                    IsAdmin = p.ProjectUsers.Any(u => u.UserId == _userContextService.GetUserId() && u.IsAdmin)
                }).SingleOrDefaultAsync(p => p.project.Id == projectId);

            if (project == null)
                return new ServiceResponse<string>("Project does not exist");
            if (!project.IsAdmin)
                return new ServiceResponse<string>("You are not the admin");


            _context.Remove(project.project);
            await _context.SaveChangesAsync();
            return new ServiceResponse<string>("Project deleted successfully");
        }
    }
}
