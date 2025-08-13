namespace SSToDo.Models.Dtos
{
    public class AuthDtos
    {
        public class LoginDto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class AuthResultDto
        {
            public string Token { get; set; }
            public DateTime ExpireAt { get; set; }
        }
    }
}
