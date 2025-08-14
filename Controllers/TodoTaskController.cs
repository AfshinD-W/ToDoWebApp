using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSToDo.Models.Dtos;
using SSToDo.Services;

namespace SSToDo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TodoTaskController : ControllerBase
    {
        private readonly ITodoTaskService _todoTaskService;

        public TodoTaskController(ITodoTaskService todoTaskService)
        {
            _todoTaskService = todoTaskService;
        }

        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTaskAsync([FromBody] CreateTodoTaskDto dto, int projectId)
        {
            var result = await _todoTaskService.CreateTodoTaskAsync(dto, projectId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPut("update-task/{taskId}")]
        public async Task<IActionResult> UpdateTaskAsync([FromBody] UpdateTodoTaskDto dto, int taskId)
        {
            var result = await _todoTaskService.UpdateTaskAsync(dto, taskId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpDelete("delete-task/{taskId}")]
        public async Task<IActionResult> DeleteTaskAsync(int taskId)
        {
            var result = await _todoTaskService.DeleteTodoTaskAsync(taskId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }
    }
}
