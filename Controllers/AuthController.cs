using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Services;
using SSToDo.Utilities;
using static SSToDo.Models.Dtos.AuthDtos;

namespace SSToDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IHashPasswordService _hashPasswordService;

        public AuthController(AppDbContext context, IJwtService jwtService, IConfiguration configuration, IHashPasswordService hashPasswordService)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
            _hashPasswordService = hashPasswordService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.UserName || u.Name == dto.UserName);
            if (user == null) return Unauthorized("Invalid credentials");

            var verifyPassword = _hashPasswordService.Verify(dto.Password, user.Password);

            if (!verifyPassword) return Unauthorized("Invalid credentials");

            var token = _jwtService.GenerateToken(user);
            return Ok(new AuthResultDto
            {
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]))
            });
        }
    }
}
