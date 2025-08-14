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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUserId = _userContextService.GetUserId();
                var isAdmin = task.Project.ProjectUsers.Any(u => u.UserId == currentUserId && u.IsAdmin);

                if (isAdmin)
                {
                    if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId != task.AssignedToUserId)
                    {
                        var assignedUserIsMember = task.Project.ProjectUsers.Any(u => u.UserId == dto.AssignedToUserId);

                        if (!assignedUserIsMember)
                            return new ServiceResponse<TodoTask>("This user is not a member of this project.");

                        task.AssignedToUserId = dto.AssignedToUserId;
                    }

                    if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != task.Title)
                        task.Title = dto.Title;

                    if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description != task.Description)
                        task.Description = dto.Description;

                    if (dto.StartDate.HasValue && dto.StartDate.Value != task.StartDate)
                        task.StartDate = dto.StartDate.Value;

                    if (dto.DueDate.HasValue && dto.DueDate.Value != task.DueDate)
                        task.DueDate = dto.DueDate.Value;

                    if (dto.Priority.HasValue && dto.Priority != task.Priority)
                        task.Priority = dto.Priority.Value;
                }
                else
                {
                    return new ServiceResponse<TodoTask>("Only admin can change this parameters.");
                }

                if (dto.Status.HasValue && dto.Status != task.Status)
                {
                    var oldStatus = task.Status;
                    var taskHistory = await _context.TaskHistories.FirstOrDefaultAsync(h => h.TaskId == task.Id);

                    if (!isAdmin && task.StartDate < DateTime.UtcNow)
                        return new ServiceResponse<TodoTask>("Task time is over you cant change it now.");

                    if(!isAdmin && task.Status == TaskStatusEnums.Approved)
                        return new ServiceResponse<TodoTask>("Task is Approved.");

                    task.Status = dto.Status.Value;

                    if (taskHistory == null)
                    {
                        var newTaskHistory = new TaskHistory
                        {
                            TaskId = task.Id,
                            ChangedByUserId = currentUserId,
                            OldStatus = oldStatus,
                            NewStatus = task.Status
                        };

                        await _context.TaskHistories.AddAsync(newTaskHistory);
                    }
                    else
                    {
                        taskHistory.OldStatus = oldStatus;
                        taskHistory.NewStatus = task.Status;
                    }

                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResponse<TodoTask>(task);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ServiceResponse<TodoTask>(ex.Message);
            }
        }

        //Delete Todo
        public async Task<ServiceResponse<TodoTask>> DeleteTodoTaskAsync()
        {
            throw new NotImplementedException();
        }
    }
}
