using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Api.Authentication.Dto;

namespace Api.Api.Authentication
{
    public interface IAuthenticationService
    {
        Task<int> RegisterUser(RegisterUserDto inData);
        Task<LoginOutDto> Login(LoginInDto inDto);
        Task<List<UserDto>> GetUsers();
        Task<UserDto> GetUser(int userId);
    }
}
