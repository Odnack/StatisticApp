using System;
using System.Threading.Tasks;
using Api.Api.Authentication.Dto;
using BackendCore.Authentication;
using Serilog;

namespace BackendCore.Helpers
{
    public class DefaultDbCreator
    {
        private readonly BackendCore _core;
        private readonly AuthenticationService _authentication;
        public DefaultDbCreator(BackendCore core)
        {
            _core = core;
            _authentication = core.InternalAuthentication;
        }
        public static async Task Setup(BackendCore core)
        {
            var thisObj = new DefaultDbCreator(core);
            await thisObj.SetupNonStatic();
        }

        private async Task SetupNonStatic()
        {
            var usersCount = (await _authentication.GetUsers()).Count;
            if (usersCount == 0)
            {
                await CreateUsers();
            }
        }

        private async Task CreateUsers()
        {
            Log.Information("Initial users and roles is starting.");

            var adminUserData = new RegisterUserDto
            {
                Password = "admin",
                Email = "admin@admin.admin"
            };

            await _authentication.RegisterUserNoCredentialCheck(adminUserData);
        }
    }
}