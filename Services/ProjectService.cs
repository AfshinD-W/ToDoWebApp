using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Utilities;

namespace SSToDo.Services
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<Project>>> GetProjectsAsync();
        Task<ServiceResponse<List<TodoTask>>> GetProjectTodoTasksAsync(int projectId);
        Task<ServiceResponse<ResponseProjectDto>> CreateProjectAsync(CreateProjectDto project);
        Task<ServiceResponse<ResponseProjectDto>> UpdateProjectAsync(UpdateProjectDto project, int projectId);
        Task<ServiceResponse<List<int>>> AddMemberToProjectAsync(List<int> memberIds, int projectId);
        Task<ServiceResponse<string>> RemoveMemberFromProjectAsync(List<int> memberIds, int projectId);
        Task<ServiceResponse<string>> DeleteProjectAsync(int projectId);
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public ProjectService(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
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
            var projectTasks = await _context.TodoTasks.AsNoTracking()
                .Where(t => t.ProjectId == projectId &&
                        t.Project.ProjectUsers.Any(u => u.UserId == _userContextService.GetUserId()))
                .ToListAsync();

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

        //Add member to project
        public async Task<ServiceResponse<List<int>>> AddMemberToProjectAsync(List<int> memberIds, int projectId)
        {
            if (memberIds == null || projectId == null)
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

            var newUsersToAdd = new List<ProjectUser>();

            foreach (var userId in memberIds)
            {
                if (!project.ExistingUserIds.Contains(userId))
                    newUsersToAdd.Add(new ProjectUser
                    {
                        ProjectId = projectId,
                        UserId = userId
                    });
            }

            if (newUsersToAdd.Any())
            {
                await _context.ProjectUsers.AddRangeAsync(newUsersToAdd);
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
