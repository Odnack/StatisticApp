using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Api;
using Api.Api.Authentication;
using Api.Api.Authentication.Dto;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Statistic.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;


namespace Statistic.Controllers
{
    [Authorize]
    public class AuthenticationController : BaseController, Api.Api.Authentication.IAuthenticationService
    {
        private Api.Api.Authentication.IAuthenticationService Authentication => BackendApi.Authentication;
        private Api.Api.Authentication.IAuthenticationService AuthenticationAnonymous => BackendApiAnonymous.Authentication;
        public AuthenticationController(IBackendApi backendApi) : base(backendApi)
        {
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public async Task<int> RegisterUser(RegisterUserDto inData)
        {
            var result = await Authentication.RegisterUser(inData);
            
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterUserDto inData)
        {
            var result = await RegisterUser(inData);
            if (result == -1)
            {
                ViewData["RegistrationErrorCode"] = "-1";
                return View();
            }

            return await LogIn(new LoginInDto() {Email = inData.Email, Password = inData.Password});
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LogIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginInDto inDto)
        {
            var result = Login(inDto).Result;
            if (result == null)
            {
                ViewData["LoginError"] = true;
                return View();
            }
            await Authenticate(inDto.Email);
            return RedirectToAction("Index", "Home");
        }
        public async Task<LoginOutDto> Login(LoginInDto inDto)
        {
            var result = await AuthenticationAnonymous.Login(inDto);
            
            return result;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await Authentication.GetUsers();
        }

        public async Task<UserDto> GetUser(int userId)
        {
            return await Authentication.GetUser(userId);
        }
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home"); 
        }
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), authProperties);
        }
    }
}