using Microsoft.AspNetCore.Mvc;
using SSToDo.Models.Dtos;
using SSToDo.Services;

namespace SSToDo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDtos dto)
        {
            var result = await _userService.CreateUserAsync(dto);

            if (result.Data == null)
                return BadRequest(result.Message);
            return Ok(result);

        }

    }
}
