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
        Task<ServiceResponse<TodoTask>> CreateTodoTasksAsync(CreateTodoTaskDto dto, int projectId);
        Task<ServiceResponse<TodoTask>> UpdateTasksAsync();
        Task<ServiceResponse<TodoTask>> DeleteTodoTasksAsync();

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
        public async Task<ServiceResponse<TodoTask>> CreateTodoTasksAsync(CreateTodoTaskDto dto, int projectId)
        {
            var membership = await _context.ProjectUsers
                .Where(u => u.ProjectId == projectId && u.UserId == _userContextService.GetUserId())
                .FirstOrDefaultAsync();

            if (membership == null)
                return new ServiceResponse<TodoTask>("You are not a member of this project.");

            if (!membership.IsAdmin)
                return new ServiceResponse<TodoTask>("Only admin of project can create task for it.");

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
        public async Task<ServiceResponse<TodoTask>> DeleteTodoTasksAsync()
        {
            throw new NotImplementedException();
        }

        //Delete Todo
        public async Task<ServiceResponse<TodoTask>> UpdateTasksAsync()
        {
            throw new NotImplementedException();
        }
    }
}
