using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Api.Authentication;
using Api.Api.Authentication.Dto;
using RestClient.Helpers;

namespace RestClient.Clients
{
    public class AuthenticationRest : IAuthenticationService
    {
        public const string ServiceUrl = "Authentication";
        private readonly RestHelper _helper;
        public AuthenticationRest(RestHelper helper)
        {
            _helper = helper;
        }

        public async Task<int> RegisterUser(RegisterUserDto inData)
        {
            var result = await _helper.ExecutePost<int>(ServiceUrl, "RegisterUser", inData, true);
            inData.Id = result;
            return result;
        }

        public async Task<LoginOutDto> Login(LoginInDto inDto)
        {
            var result = await _helper.ExecutePostAnonymous<LoginOutDto>(ServiceUrl, "Login", inDto, true);
            _helper.SetUserToken(result.UserId, result.Token);
            _helper.CurrUserId = result.UserId;
            return result;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await _helper.ExecuteGet<List<UserDto>>(ServiceUrl, "GetUsers");
        }

        public async Task<UserDto> GetUser(int userId)
        {

            return await _helper.ExecuteGet<UserDto>(ServiceUrl, "GetUser", new Tuple<string, string>("userId", userId.ToString()));
        }
    }
}