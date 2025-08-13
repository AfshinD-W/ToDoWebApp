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
        Task<ServiceResponse<ResponseProjectDto>> CreateProjectAsync(CreateProjectDto project);
        Task<ServiceResponse<ResponseProjectDto>> UpdateProjectAsync(UpdateProjectDto project, int projectId);
        Task<ServiceResponse<List<int>>> AddMemberToProjectAsync(List<int> userIds, int projectId);
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

        //AddMember to project
        public async Task<ServiceResponse<List<int>>> AddMemberToProjectAsync(List<int> userIds, int projectId)
        {
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

            foreach (var userId in userIds)
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
