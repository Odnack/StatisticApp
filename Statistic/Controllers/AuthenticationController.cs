using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Api;
using Api.Api.Authentication.Dto;
using DbLayer.Tables.Public;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Statistic.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using IAuthenticationService = Api.Api.Authentication.IAuthenticationService;


namespace Statistic.Controllers
{
    public class AuthenticationController : BaseController, IAuthenticationService
    {
        private IAuthenticationService Authentication => BackendApi.Authentication;
        public AuthenticationController(IBackendApi backendApi) : base(backendApi)
        {
        }
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("HomePage", "NotificationApplication");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterUserDto inData)
        {
            var result = await Authentication.SignUp(inData);
            if (result is OkObjectResult okObject)
            {
                var user = okObject.Value as User;
                return await LogIn(new LoginInDto(){Email = user.Email, Password = user.Password});
            }
            ViewData["RegistrationErrorCode"] = "-1";
            return View();
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            if (User.Identity.IsAuthenticated)
             return RedirectToAction("HomePage", "NotificationApplication");
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginInDto inDto)
        {
            var result = await Authentication.LogIn(inDto);
            if (result is OkObjectResult okObject)
            {
                var user = okObject.Value as LoginOutDto;
                await Authenticate(user);
                return RedirectToAction("HomePage", "NotificationApplication");
            }
            ViewData["LoginError"] = true;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            if (User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("HomePage", "NotificationApplication");
        }
        private async Task Authenticate(LoginOutDto user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.UserId)),
                new Claim(ClaimTypes.Name, user.Email)
            };
            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var identity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties() { IsPersistent = true };
            await HttpContext.SignInAsync(scheme, principal, properties);
        }
    }
}