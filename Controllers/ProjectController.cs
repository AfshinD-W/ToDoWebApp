using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSToDo.Models.Dtos;
using SSToDo.Services;

namespace SSToDo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("get-projects")]
        public async Task<IActionResult> GetProjectsAsync()
        {
            var result = await _projectService.GetProjectsAsync();

            if (result != null)
                return Ok(result);
            return BadRequest(new { message = "there is no project for you" });
        }

        [HttpGet("get-project-tasks/{projectId}")]
        public async Task<IActionResult> GetProjectTodoTasksAsync(int projectId)
        {
            var result = await _projectService.GetProjectTodoTasksAsync(projectId);

            if (result != null)
                return Ok(result);
            return BadRequest(new { message = "there is no project for you" });
        }

        [HttpPost("create-project")]
        public async Task<IActionResult> CreateProjectAsync([FromBody] CreateProjectDto dto)
        {
            var result = await _projectService.CreateProjectAsync(dto);

            if (result == null)
                return BadRequest(new { statusCode = 404, message = "we couldent create your project pls try again" });
            return Ok(result);
        }

        [HttpPut("update-project/{projectId}")]
        public async Task<IActionResult> UpdateProjectAsync([FromBody] UpdateProjectDto dto, int projectId)
        {
            var result = await _projectService.UpdateProjectAsync(dto, projectId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("add-members/{projectId}")]
        public async Task<IActionResult> InviteMemberToProjectAsync([FromBody] List<int> memberIds, int projectId)
        {
            var result = await _projectService.InviteMemberToProjectAsync(memberIds, projectId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpGet("invite")]
        public async Task<IActionResult> ConfirmInviteAsync([FromQuery] string inviteToken)
        {
            var result = await _projectService.ConfirmInviteAsync(inviteToken);

            if (!result)
                return Redirect("/invites/failed.html");
            return Redirect("/invites/success.html");
        }

        [HttpDelete("remove-members/{projectId}")]
        public async Task<IActionResult> RemoveMemberFromProjectAsync([FromBody] List<int> memberIds, int projectId)
        {
            var result = await _projectService.RemoveMemberFromProjectAsync(memberIds, projectId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpDelete("delete-project/{projectId}")]
        public async Task<IActionResult> DeleteProjectAsync(int projectId)
        {
            var result = await _projectService.DeleteProjectAsync(projectId);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);
        }
    }
}
