using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;
using SSToDo.Utilities;

namespace SSToDo.Services
{
    public interface ITodoTaskService
    {
        Task<ServiceResponse<TodoTask>> CreateTodoTaskAsync(CreateTodoTaskDto dto, int projectId);
        Task<ServiceResponse<TodoTask>> UpdateTaskAsync(UpdateTodoTaskDto dto, int taskId);
        Task<ServiceResponse<TodoTask>> DeleteTodoTaskAsync();

    }

    public class TodoTaskService : ITodoTaskService
    {
        private readonly AppDbContext _context;
        private readonly UserContextService _userContextService;

        public TodoTaskService(AppDbContext context, UserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        //Create Todo
        public async Task<ServiceResponse<TodoTask>> CreateTodoTaskAsync(CreateTodoTaskDto dto, int projectId)
        {
            var projectMembers = await _context.ProjectUsers
                .Where(u => u.ProjectId == projectId)
                .Select(u => new { u.UserId, u.IsAdmin })
                .ToListAsync();

            var currentUserMembership = projectMembers.FirstOrDefault(u => u.UserId == _userContextService.GetUserId());

            if (currentUserMembership == null)
                return new ServiceResponse<TodoTask>("You are not a member of this project.");

            if (!currentUserMembership.IsAdmin)
                return new ServiceResponse<TodoTask>("Only admin of project can create task for it.");

            if (dto.AssignedToUserId.HasValue && !projectMembers.Any(u => u.UserId == dto.AssignedToUserId.Value))
                return new ServiceResponse<TodoTask>("Assigned user is not a member of this project.");

            var todoTask = new TodoTask
            {
                ProjectId = projectId,
                Title = dto.Title,
                Description = dto.Description,
                AssignedToUserId = dto.AssignedToUserId,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                Status = TaskStatusEnums.Open,
            };

            await _context.TodoTasks.AddAsync(todoTask);
            await _context.SaveChangesAsync();

            return new ServiceResponse<TodoTask>(todoTask);
        }

        //Update Todo
        public async Task<ServiceResponse<TodoTask>> UpdateTaskAsync(UpdateTodoTaskDto dto, int taskId)
        {
            var task = await _context.TodoTasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectUsers)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return new ServiceResponse<TodoTask>("Task not found");

            var currentUserId = _userContextService.GetUserId();
            var isAdmin = task.Project.ProjectUsers.Any(u => u.UserId == currentUserId && u.IsAdmin);

            if (task.AssignedToUserId != currentUserId && !isAdmin)
                return new ServiceResponse<TodoTask>("Only admin or assigned user can change the task.");

        }

        //Delete Todo
        public async Task<ServiceResponse<TodoTask>> DeleteTodoTaskAsync()
        {
            throw new NotImplementedException();
        }
    }
}
