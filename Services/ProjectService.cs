using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Entity;

namespace SSToDo.Services
{

    public interface IProjectService
    {
        Task<List<Project>> GetUserProjectsAsync(int userId);
        Task<Project> CreateProjectAsync(string name, string description, int creatorUserId);
        Task<bool> AddUserToProjectAsync(int projectId, int userIdToAdd, int actingUserId);
    }


    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetUserProjectsAsync(int userId)
        {
            return await _context.tblProjectUsers
                .Where(pu => pu.UserId == userId)
                .Select(pu => pu.Project)
                .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(string name, string description, int creatorUserId)
        {
            var project = new Project
            {
                Name = name,
                Description = description,
                CreatedById = creatorUserId,
                ProjectUsers = new List<ProjectUser>
            {
                new ProjectUser { UserId = creatorUserId }
            }
            };

            _context.tblProjects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> AddUserToProjectAsync(int projectId, int userIdToAdd, int actingUserId)
        {
            var project = await _context.tblProjects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.CreatedById != actingUserId)
                return false;

            if (project.ProjectUsers.Any(pu => pu.UserId == userIdToAdd))
                return false;

            project.ProjectUsers.Add(new ProjectUser { ProjectId = projectId, UserId = userIdToAdd });
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
