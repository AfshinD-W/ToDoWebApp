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
        private readonly IFileService _fileService;
        private readonly static string _imageFolderForUser = "TodoProfileImage";

        public UserService(AppDbContext context, IHashPasswordService hashPasswordService, IFileService fileService)
        {
            _context = context;
            _hashPasswordService = hashPasswordService;
            _fileService = fileService;
        }

        public async Task<ServiceResponse<ResponseUserDtos>> CreateUserAsync(CreateUserDtos dto)
        {
            var existsUserEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            string? imagePath = dto.Image != null ? await _fileService.AddImageAsync(dto.Image, _imageFolderForUser) : null ;

            if (existsUserEmail)
                return new ServiceResponse<ResponseUserDtos>("There is an user with this email allready");

            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = _hashPasswordService.Hash(dto.Password),
                Age = dto.Age,
                ImagePath = imagePath
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
