using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Api.Authentication.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Authentication
{
    public interface IAuthenticationService
    {
        Task<IActionResult> SignUp(RegisterUserDto inData);
        Task<IActionResult> LogIn(LoginInDto inDto);


    }
}
