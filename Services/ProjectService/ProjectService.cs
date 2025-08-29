using SSToDo.Data;
using SSToDo.Models.Dtos;
using SSToDo.Models.Entities;
using SSToDo.Shared;
using SSToDo.Utilities;

namespace SSToDo.Services.ProjectService
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<Project>>> GetProjectsAsync();
        Task<ServiceResponse<List<TodoTask>>> GetProjectTodoTasksAsync(int projectId);
        Task<ServiceResponse<ResponseProjectDto>> CreateProjectAsync(CreateProjectDto project);
        Task<ServiceResponse<ResponseProjectDto>> UpdateProjectAsync(UpdateProjectDto project, int projectId);
        Task<ServiceResponse<List<int>>> InviteMemberToProjectAsync(List<int> memberIds, int projectId);
        Task<bool> ConfirmInviteAsync(string inviteToken);
        Task<ServiceResponse<string>> RemoveMemberFromProjectAsync(List<int> memberIds, int projectId);
        Task<ServiceResponse<string>> DeleteProjectAsync(int projectId);
    }

    public partial class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IHashPasswordService _hashPasswordService;
        private readonly IEmailService _emaileService;
        private readonly IConfiguration _configuration;

        public ProjectService(AppDbContext context, IUserContextService userContextService, IHashPasswordService hashPasswordService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _userContextService = userContextService;
            _emaileService = emailService;
            _configuration = configuration;
        }
    }
}
