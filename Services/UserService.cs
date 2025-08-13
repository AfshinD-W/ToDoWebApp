using Microsoft.EntityFrameworkCore;
using SSToDo.Data;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Utilities;

namespace SSToDo.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<ResponseUserDtos>> CreateUserAsync(CreateUserDtos dto);

    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IHashPasswordService _hashPasswordService;

        public UserService(AppDbContext context, IHashPasswordService hashPasswordService)
        {
            _context = context;
            _hashPasswordService = hashPasswordService;
        }

        public async Task<ServiceResponse<ResponseUserDtos>> CreateUserAsync(CreateUserDtos dto)
        {
            var existsUserEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);

            if (existsUserEmail)
                return new ServiceResponse<ResponseUserDtos>("There is an user with this email allready");

            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = _hashPasswordService.Hash(dto.Password),
                Age = dto.Age,
                ImagePath = dto.ImagePath
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var response = new ResponseUserDtos
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                ConfirmedEmail = newUser.ConfirmedEmail,
                Age = newUser.Age,
                ImagePath = newUser.ImagePath,
            };

            return new ServiceResponse<ResponseUserDtos>(response);
        }
    }
}
