namespace SSToDo.Models.Dtos
{
    public class CreateUserDtos
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? Age { get; set; }
        public string? ImagePath { get; set; }
    }

    public class ResponseUserDtos
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool ConfirmedEmail { get; set; }
        public int? Age { get; set; }
        public string? ImagePath { get; set; }
    }
}
