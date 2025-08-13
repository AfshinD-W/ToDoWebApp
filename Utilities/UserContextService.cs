using System.Security.Claims;

namespace SSToDo.Utilities
{
    public interface IUserContextService
    {
        int GetUserId();
    }
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int GetUserId()
        {
            var climValue = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(climValue, out int userId))
                return userId;

            return 0;
        }
    }
}
