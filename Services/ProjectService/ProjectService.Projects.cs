using Microsoft.EntityFrameworkCore;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;
using SSToDo.Utilities;

namespace SSToDo.Services.ProjectService
{
    public partial class ProjectService
    {
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
