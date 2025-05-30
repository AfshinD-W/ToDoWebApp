using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Dtos;
using SSToDo.Entity;
using TaskStatusEnum = SSToDo.Models.Enums.TaskStatus;

namespace SSToDo.Services
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTaskAsync(TaskCreateDto dto, int creatorUserId);
        Task<bool> UpdateTaskStatusAsync(int taskId, int userId, bool isDone);
        Task<bool> ReopenTaskAsync(int taskId, int actingUserId);
        Task<List<TaskItem>> GetTasksForProjectAsync(int projectId, int userId);
        Task<List<TaskItem>> GetTasksForUserAsync(int userId);
    }

    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> CreateTaskAsync(TaskCreateDto dto, int creatorUserId)
        {
            var project = await _context.tblProjects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null || project.CreatedById != creatorUserId)
                throw new UnauthorizedAccessException("Only project admin can create tasks.");

            if (!project.ProjectUsers.Any(pu => pu.UserId == dto.AssignedToUserId))
                throw new Exception("Assigned user is not in the project.");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                AssignedToId = dto.AssignedToUserId,
                DurationDays = dto.DurationDays,
                Priority = dto.Priority,
                Status = TaskStatusEnum.Open
            };

            _context.tblTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int userId, bool isDone)
        {
            var task = await _context.tblTasks.FindAsync(taskId);

            if (task == null || task.AssignedToId != userId)
                return false;

            if (task.Status == TaskStatusEnum.Completed)
                return false;

            task.Status = isDone ? TaskStatusEnum.Completed : TaskStatusEnum.Open;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReopenTaskAsync(int taskId, int actingUserId)
        {
            var task = await _context.tblTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.Project.CreatedById != actingUserId)
                return false;

            task.Status = TaskStatusEnum.Reopened;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TaskItem>> GetTasksForProjectAsync(int projectId, int userId)
        {
            var isMember = await _context.tblProjectUsers
                .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not part of this project.");

            return await _context.tblTasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetTasksForUserAsync(int userId)
        {
            return await _context.tblTasks
                .Where(t => t.AssignedToId == userId)
                .ToListAsync();
        }
    }
}
