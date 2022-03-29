namespace Api.Api.Authentication.Dto
{
    public class LoginOutDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}