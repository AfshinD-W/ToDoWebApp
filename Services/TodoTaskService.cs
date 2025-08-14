using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Models.Entities;
using SSToDo.Utilities;

namespace SSToDo.Services
{
    public interface ITodoTaskService
    {
        Task<ServiceResponse<List<TodoTask>>> GetTodoTasksAsync();
        Task<ServiceResponse<TodoTask>> CreateTodoTasksAsync();
        Task<ServiceResponse<TodoTask>> UpdateTasksAsync();
        Task<ServiceResponse<TodoTask>> DeleteTodoTasksAsync();

    }

    public class TodoTaskService : ITodoTaskService
    {
        private readonly AppDbContext _context;

        public TodoTaskService(AppDbContext context)
        {
            _context = context;
        }


        //Get Todos
        public async Task<ServiceResponse<List<TodoTask>>> GetTodoTasksAsync()
        {
            //var todoTasks = _context.TodoTasks.AsNoTracking().Where()

            throw new NotImplementedException();
        }

        //Create Todo
        Task<ServiceResponse<TodoTask>> ITodoTaskService.CreateTodoTasksAsync()
        {
            throw new NotImplementedException();
        }

        //Update Todo
        Task<ServiceResponse<TodoTask>> ITodoTaskService.DeleteTodoTasksAsync()
        {
            throw new NotImplementedException();
        }

        //Delete Todo
        Task<ServiceResponse<TodoTask>> ITodoTaskService.UpdateTasksAsync()
        {
            throw new NotImplementedException();
        }
    }
}
